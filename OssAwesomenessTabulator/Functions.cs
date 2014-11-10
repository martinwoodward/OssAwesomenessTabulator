using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OssAwesomenessTabulator.Data;
using Octokit;

namespace OssAwesomenessTabulator
{
    public class Functions
    {

        public static OssData GetData(Config config)
        {
            OssData data = new OssData();
            CodePlexUtils codeplex = new CodePlexUtils();

            IList<Org> orgs = config.GetOrgs();

            // Get the orgs
            foreach (Org org in orgs)
            {
                try
                {
                    data.AddProjects(GitHubUtils.GetGitHubProjects(org, config.GitHubCredentials).Result.ToArray());
                }
                catch (Exception ex)
                {
                    // Yeah, I know this is bad, but want to try hard for each org
                    // Consider throwing if not one of a set of defined types.
                    System.Diagnostics.Trace.TraceError("Exception detected in org \"{0}\": {1}", org.Name, ex.StackTrace);
                }                
            }

            if (config.CodePlexUsers != null && config.CodePlexUsers.Length > 0)
            {
                // We've been configured to crawl some CodePlex users (i.e. Microsoft & MSOpenTech)
                foreach (string user in config.CodePlexUsers)
                {
                    try
                    {
                        data.AddProjects(codeplex.GetProjects(user));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError("Exception detected in codeplex org \"{0}\": {1}", user, ex.StackTrace);
                    }
                }
            }

            // Now we've loaded the projects from the orgs, let's load up the individual projects to load / update
            IList<Project> projects = config.GetProjects();
            foreach (Project project in projects)
            {
                Project existing = data.GetProject(project);
                if (existing != null)
                {
                    // Update project
                    existing.Update(project);
                }
                else if (project.isGitHub())
                {
                    try
                    {
                        data.AddProject(GitHubUtils.GetGitHubProject(project, config.GitHubCredentials).Result);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError("Exception detected in project \"{0}/{1}\": {2}", project.GithubOrg, project.GithubRepo, ex.StackTrace);
                    }
                }
                else if (String.IsNullOrEmpty(project.CodePlexProject))
                {
                    data.AddProject(codeplex.GetProject(project));
                }
            }

            return (data);
        }

        public static void Write( 
            [Blob("output/{name}.json", FileAccess.Write)] Stream output,
            OssData data)
        {
            using (StreamWriter sw = new StreamWriter(output, Encoding.UTF8))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;

                JsonSerializer js = JsonSerializer.Create(GetSettings());
                js.Serialize(jw, data);
            }
        }

        public static void WriteTop(
            [Blob("output/{name}_top.json", FileAccess.Write)] Stream output,
            OssData data)
        {
            Write(output, data.Top(50));
        }

        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings {
              NullValueHandling = NullValueHandling.Ignore,
              DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
        }

    }
}
