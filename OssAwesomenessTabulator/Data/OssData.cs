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

        private List<Project> _projects = new List<Project>();
        public void AddProject(Project project)
        {
            _projects.Add(project);
            Summary.Projects = _projects.Count;

        }
        public void AddProjects(Project[] projects)
        {
            foreach (Project project in projects)
            {
                AddProject(project);
            }
        }

        public OssData()
        {
            Summary = new SummaryStats();
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

    }
}
