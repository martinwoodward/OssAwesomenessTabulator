using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace OssAwesomenessTabulator.Data
{
    [DataContract]
    public class OssData
    {
        private List<Project> _projects = new List<Project>();
        private HashSet<string> _orgs = new HashSet<string>();
        private string _defaultContributor;
        
        // Included in JSON
        [DataMember]
        public DateTimeOffset LastUpdated { get { return DateTimeOffset.Now; } }

        [DataMember]
        public SummaryStats Summary { get; set; }

        [DataMember]
        public Project[] Projects
        {
            get
            {
                // Return the projects in order of awesomeness then last commit date
                return _projects
                    .OrderByDescending(project => project.Awesomeness)
                    .ThenByDescending(project => project.CommitLast)
                    .ToArray<Project>();
            }
        }

        public void AddProject(Project project)
        {
            if (project.License != null && project.License.Type == "Custom License")
            {
                // Probably MS-LPL or MSRLT so not really Open Source as per OSI definition
                return;
            }

            if (!String.IsNullOrEmpty(project.Description)
                && !String.IsNullOrEmpty(project.Name)
                && (
                     project.Description.ToLower().Contains(" moved")
                  || project.Description.ToLower().Contains(" mirror")
                  || project.Name.ToLower().Contains("obsolete")
                  || project.Name.ToLower().Contains("moved")
                  ))
            {
                // The stats for these were valid to the whole, but we probably have a better
                // project to show in the place of these ones.
                return;
            }

            if (String.IsNullOrEmpty(project.Contributor))
            {
                project.Contributor = _defaultContributor;
            }

            // give it an ID
            project.Id = _projects.Count + 1;

            _projects.Add(project);

            Summary.Projects = _projects.Count;
            Summary.Contributors += project.Contributors;
            Summary.OpenIssues += project.OpenIssues;
            Summary.Forks += project.Forks;
            Summary.Stars += project.Stars;

            if (!String.IsNullOrEmpty(project.GithubOrg))
            {
                _orgs.Add(project.GithubOrg);
                Summary.Organizations = _orgs.Count;
            }            
        }
        public void AddProjects(Project[] projects)
        {
            foreach (Project project in projects)
            {
                AddProject(project);
            }
        }
        public Project GetProject(Project project)
        {
            // This code is bad in many many way. Need to refactor.

            bool found = false;
            Project foundProject = null;
            // Check to see if project already exists in data
            foreach (Project p in _projects)
            {
                if (p.isGitHub())
                {
                    // looking for a GitHub Project
                    if (project.GithubRepo == p.GithubRepo
                        && project.GithubOrg == p.GithubOrg)
                    {
                        found = true;
                    }
                }
                else if (!String.IsNullOrEmpty(project.CodePlexProject))
                {
                    // looking for a CodePlex Project
                    if (project.CodePlexProject == p.CodePlexProject)
                    {
                        found = true;
                    }
                }
                else
                {
                    // look for a match using URL
                    if (p.Url == project.Url)
                    {
                        found = true;
                    }
                }
                if (found)
                {
                    foundProject = p;
                    break;
                }
            }
            return foundProject;
        }


        public OssData() : this(null) {}
        public OssData(string defaultContributor)
        {
            Summary = new SummaryStats();
            _defaultContributor = defaultContributor;
        }

        public OssData Top(int count)
        {
            OssData data = new OssData();
            data.AddProjects(_projects
                    .OrderByDescending(project => project.Awesomeness)
                    .ThenByDescending(project => project.CommitLast)
                    .Take(count)
                    .ToArray());
            data.Summary = this.Summary;
            return data;
        }

        public OssData Active()
        {
            OssData data = new OssData();
            data.AddProjects(_projects
                    .OrderByDescending(project => project.Awesomeness)
                    .ThenByDescending(project => project.CommitLast)
                    .Where(project => project.CommitLast > DateTimeOffset.Now.Subtract(TimeSpan.FromDays(365)))
                    .ToArray());
            data.Summary = this.Summary;
            return data;
        }

        public OssData GitHub()
        {
            OssData data = new OssData();
            data.AddProjects(_projects
                    .OrderByDescending(project => project.Awesomeness)
                    .ThenByDescending(project => project.CommitLast)
                    .Where(project => !String.IsNullOrEmpty(project.GithubRepo))
                    .ToArray());
            data.Summary = this.Summary;
            return data;
        }

    }
}
