using OssAwesomenessTabulator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OssAwesomenessTabulator.CodePlex;
using System.Xml.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Globalization;
using System.ServiceModel.Syndication;
using System.Xml;

namespace OssAwesomenessTabulator
{
    public class CodePlexUtils
    {
        private ProjectInfoServiceSoapClient _codeplexClient = null;
        private ProjectInfoServiceSoapClient codeplex
        {
            get
            {
                if (_codeplexClient == null)
                {
                    _codeplexClient = new ProjectInfoServiceSoapClient();
                }
                return _codeplexClient;
            }
        }

        public Project GetProject(Project project)
        {
            // Note that this is some pretty ugly code and very innefficient.
            // I'll fix up an API server side to make this easier to code but also
            // reduce the load on the server.

            // Get project name;
            Uri uri = new Uri(project.CodePlexProject);
            string name = uri.Host.Split('.')[0];
            
            var cp = codeplex.GetProjectByName(name);

            if (String.IsNullOrEmpty(project.Name))
            {
                project.Name = (string)cp.Element("ProjectTitle");
            }
            if (String.IsNullOrEmpty(project.Url))
            {
                project.Url = "http://" + name + ".codeplex.com";
            }
            if (String.IsNullOrEmpty(project.Description))
            {
                project.Description = ((string)cp.Element("ProjectDescription")).Trim();
            }
            License license = new License
            {
                Type = (string)cp.Element("LicenseName"),
                Url = "http://" + name + ".codeplex.com/license"
            };
            if (project.License == null)
            {
                project.License = license;
            }
            if (String.IsNullOrEmpty(project.License.Type))
            {
                project.License.Type = license.Type;
            }
            if (String.IsNullOrEmpty(project.License.Url))
            {
                project.License.Url = license.Url;
            }
            string date = (string)cp.Element("ProjectCreatedDate");
            project.Created = DateTimeOffset.Parse(date, new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AssumeUniversal);

            var projectUpdates = SyndicationFeed.Load(XmlReader.Create(String.Format("http://{0}.codeplex.com/project/feeds/rss",name)));
            project.Updated = projectUpdates.Items.First().PublishDate;

            var sourceUpdates = SyndicationFeed.Load(XmlReader.Create(String.Format("http://{0}.codeplex.com/project/feeds/rss?ProjectRSSFeed=codeplex%3a%2f%2fsourcecontrol%2f{0}", name)));
            project.CommitLast = sourceUpdates.Items.First().PublishDate;            

            using (var web = new WebClient())
            {
                // Stars = follows in CodePlex
                project.Stars = JsonConvert
                    .DeserializeObject<Followers>(
                        web.DownloadString(
                            String.Format("http://www.codeplex.com/site/api/projects/{0}/followProject",name)))
                    .TotalFollowers;

                // Look up number or forks / patches.
                int forks = 0;
                try
                {
                    string forkData = web.DownloadString(String.Format("http://{0}.codeplex.com/SourceControl/network?size=100", name));
                    int forkPos = forkData.IndexOf("<li>Forks (");
                    if (forkPos > 0)
                    {
                        forkPos += 11;
                        string forkNum = forkData.Substring(forkPos, (forkData.IndexOf(")</li>", forkPos) - forkPos));
                        Int32.TryParse(forkNum, out forks);
                    }
                    else
                    {
                        // Look for patches instead.
                        int patchEndPos = forkData.IndexOf("</span> Patches</li>");
                        if (patchEndPos > 0)
                        {
                            forkData = forkData.Substring(patchEndPos - 100, 100);
                            int patchStartPos = forkData.LastIndexOf("<span class=\"Selected\">");
                            patchStartPos += 23;
                            string patchNum = forkData.Substring(patchStartPos);
                            Int32.TryParse(patchNum, out forks);
                        }
                    }
                    project.Forks = forks;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Exception detected getting forks for codeplex project \"{0}\": {1}", name, ex.StackTrace);
                }
                
            }

            project.Awesomeness = Awesomeness.Calculate(project);
            
            return project;
        }

        public Project[] GetProjects(string username)
        {
            List<Project> projects = new List<Project>();
            // Get all the projects for a user i.e. "Microsoft" or "MSOpenTech"
            var projectNames = codeplex.ListProjectsForUser(username);
            foreach (string name in projectNames)
	        {
                try
                {
                    projects.Add(GetProject(new Project { CodePlexProject = "http://" + name + ".codeplex.com" }));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Exception detected in codeplex project \"{0}\": {1}", name, ex.StackTrace);
                }
	        }

            return projects.ToArray();
        }

    }

    class Followers
    {
        public int TotalFollowers { get; set; }
        public bool IsFollowing { get; set; }
    }
}
