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
            
            // Call the codeplex stats WEBSERVICE
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

            project.License = getLicense(project, 
                (string)cp.Element("LicenseName"), 
                "http://" + name + ".codeplex.com/license");

            // Activity dates
            string date = (string)cp.Element("ProjectCreatedDate");
            project.Created = DateTimeOffset.Parse(date, new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AssumeUniversal);

            var projectUpdates = SyndicationFeed.Load(XmlReader.Create(String.Format("http://{0}.codeplex.com/project/feeds/rss",name)));
            project.Updated = projectUpdates.Items.First().PublishDate;

            var sourceUpdates = SyndicationFeed.Load(XmlReader.Create(String.Format("http://{0}.codeplex.com/project/feeds/rss?ProjectRSSFeed=codeplex%3a%2f%2fsourcecontrol%2f{0}", name)));
            project.CommitLast = sourceUpdates.Items.First().PublishDate;            

            using (var web = new WebClient())
            {
                // Stars = use the follows API in CodePlex
                project.Stars = JsonConvert
                    .DeserializeObject<Followers>(
                        web.DownloadString(
                            String.Format("http://www.codeplex.com/site/api/projects/{0}/followProject",name)))
                    .TotalFollowers;

                // Ugly screen scrape ATM for forks and contributors
                // note, I can do this because I have the privaledge of knowing when the
                // underlying page is going to change. I'll build and API to stop
                // others having to do this in the future.
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

                    // Contributors
                    string peopleData = web.DownloadString(String.Format("http://{0}.codeplex.com/team/view", name)).ToLower();
                    int people = 0, i = 0;
                    string person = "Project Member since".ToLower();
                    while ((i = peopleData.IndexOf(person, i)) != -1)
                    {
                        i += person.Length;
                        ++people;
                    }
                    project.Contributors = people;
                    
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

        /// <summary>
        /// Check that the CodePlex site is reporting as being healthy.
        /// </summary>
        public bool IsHealthy()
        {
            bool up = true;

            string status = "";
            try
            {
                using (var web = new WebClient())
                {
                    status = web.DownloadString("http://www.codeplex.com/monitoring/corecheck.aspx");
                    if (String.IsNullOrEmpty(status))
                    {
                        up = false;
                    } else
                    {
                        up = (status.IndexOf("FAILED") < 0);
                    }
                }
                if (!up)
                {
                    Console.Out.WriteLine("-----\nCodePlex Health Warning\n-----");
                    Console.Out.WriteLine(status);
                    Console.Out.WriteLine("----------");
                    System.Diagnostics.Trace.TraceError("CodePlex site reporing issues\n-----\n{0}\n-----", status);
                }
            }
            catch (Exception ex)
            {
                up = false;
                Console.Out.WriteLine("-----\nException in CodePlex Health Warning\n-----");
                Console.Out.WriteLine(status);
                Console.Out.WriteLine("----------");
                Console.Out.WriteLine(ex.ToString());
                System.Diagnostics.Trace.TraceError("Exception checking CodePlex status: {0}", ex.StackTrace);
            }

            return up;
        }

        private License getLicense(Project project, string type, string url)
        {
            // License
            License license = new License
            {
                Type = type,
                Url = url
            };

            switch (license.Type)
            {
                case ("Apache License 2.0 (Apache)"):
                    license.Type = "Apache 2.0";
                    break;
                case ("Simplified BSD License (BSD)"):
                    license.Type = "BSD";
                    break;
                case ("Eclipse Public License (EPL)"):
                    license.Type = "EPL";
                    break;
                case ("GNU General Public License version 2 (GPLv2)"):
                    license.Type = "GPLv2";
                    break;
                case ("GNU General Public License version 3 (GPLv3)"):
                    license.Type = "GPLv3";
                    break;
                case ("GNU Lesser General Public License (LGPL)"):
                    license.Type = "LGPL";
                    break;
                case ("The MIT License (MIT)"):
                    license.Type = "MIT";
                    break;
                case ("Microsoft Public License (Ms-PL)"):
                    license.Type = "MS-PL";
                    break;
                case ("Microsoft Reciprocal License (Ms-RL)"):
                    license.Type = "MS-RL";
                    break;
                default:
                    break;
            }
            if (project.License == null)
            {
                project.License = license;
            }
            if (!String.IsNullOrEmpty(project.License.Type))
            {
                license.Type = project.License.Type;
            }
            if (!String.IsNullOrEmpty(project.License.Url))
            {
                license.Url = project.License.Url;
            }

            return license;
        }

    }

    class Followers
    {
        public int TotalFollowers { get; set; }
        public bool IsFollowing { get; set; }
    }
}
