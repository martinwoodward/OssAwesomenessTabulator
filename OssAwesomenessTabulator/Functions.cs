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
            OssData data = new OssData(config.DefaultContributor);
            GitHubUtils github = new GitHubUtils(config.GitHubCredentials);
            CodePlexUtils codeplex = new CodePlexUtils();

            Console.Out.WriteLine("Collecting OSS Data..");

            if (config.CodePlexUsers != null && config.CodePlexUsers.Length > 0)
            {
                // We've been configured to crawl some CodePlex users (i.e. Microsoft & MSOpenTech)

                // Check if CodePlex is up. If it's not, abort
                if (!codeplex.IsHealthy())
                {
                    throw new Exception("Error: Aborting run on on Codeplex Orgs as site reporting health issues");
                }

                foreach (string user in config.CodePlexUsers)
                {
                    Console.Out.WriteLine("Getting data for CodePlex user {0}", user);
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

            // Check GitHub is healthy. 
            // If it's not green back off and be cool while they recover, we don't need our stats that bad
            if (!github.isHealthy())
            {
                throw new Exception(String.Format("Error: Aborting run due to GitHub health report. See https://status.github.com/"));
            }

            IList<Org> orgs = config.GetOrgs();
            Console.Out.WriteLine("Getting data for {0} orgs", orgs.Count());
            // Get the orgs
            foreach (Org org in orgs)
            {
                try
                {
                    data.AddProjects(github.GetGitHubProjects(org).Result.ToArray());
                }
                catch (Exception ex)
                {
                    // Yeah, I know this is bad, but want to try hard for each org
                    // Consider throwing if not one of a set of defined types.
                    System.Diagnostics.Trace.TraceError("Exception detected in org \"{0}\": {1}", org.Name, ex.StackTrace);
                }                
            }


            // Now we've loaded the projects from the orgs, let's load up the individual projects to load / update
            IList<Project> projects = config.GetProjects();
            Console.Out.WriteLine("Adding data for {0} projects", projects.Count());
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
                        data.AddProject(github.GetGitHubProject(project).Result);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError("Exception detected in project \"{0}/{1}\": {2}", project.GithubOrg, project.GithubRepo, ex.StackTrace);
                    }
                }
                else if (!String.IsNullOrEmpty(project.CodePlexProject))
                {
                    data.AddProject(codeplex.GetProject(project));
                }
            }

            return (data);
        }

        public static void Write( 
            [Blob("output/{name}.json", FileAccess.Write)] Stream output,
            OssData data, string callbackFunction)
        {
            using (StreamWriter sw = new StreamWriter(output, Encoding.UTF8))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                if (!String.IsNullOrEmpty(callbackFunction))
                {
                    // Been passed a callback function so write as JSONP
                    sw.Write(callbackFunction);
                    sw.Write("([");
                    sw.Flush();
                }

                jw.Formatting = Formatting.Indented;
                JsonSerializer js = JsonSerializer.Create(GetSettings());
                js.Serialize(jw, data);

                if (!String.IsNullOrEmpty(callbackFunction))
                {
                    // Was passed a function so close JSONP callback
                    sw.Write("]);");
                }
            }
        }

        public static void WriteTop(
            [Blob("output/{name}_top.json", FileAccess.Write)] Stream output,
            OssData data)
        {
            Write(output, data.Top(50),null);
        }

        public static void WriteActive(
        [Blob("output/{name}_top.json", FileAccess.Write)] Stream output,
        OssData data)
        {
            Write(output, data.Active(), null);
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
