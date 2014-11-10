using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OssAwesomenessTabulator.Data
{
    public class Project
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int Awesomeness { get; set; }
        public string Description { get; set; }
        public string[] Keywords { get; set; }
        public string[] Tags { get; set; }
        public License License { get; set; }
        public string Language { get; set; }
        public string Contributor { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public DateTimeOffset? CommitLast { get; set; }
        public int Stars { get; set; }
        public int Forks { get; set; }
        public int Contributors { get; set; }
        public string GithubOrg { get; set; }
        public string GithubRepo { get; set; }
        public string CodePlexProject { get; set; }
        public int OpenIssues { get; set; }
        public bool IsFork { get; set;}

        public bool isGitHub()
        {
            return !(String.IsNullOrEmpty(GithubOrg) || String.IsNullOrEmpty(GithubRepo));
        }

        /// <summary>
        ///   Override the values in this project with those passed in.
        /// </summary>
        public void Update(Project project)
        {
            if (!String.IsNullOrEmpty(project.Name))
            {
                this.Name = project.Name;
            }
            if (!String.IsNullOrEmpty(project.Url))
            {
                this.Url = project.Url;
            }
            if (!String.IsNullOrEmpty(project.Description))
            {
                this.Description = project.Description;
            }
            if (project.Keywords != null)
            {
                this.Keywords = union(project.Keywords, this.Keywords);
            }
            if (project.Tags != null)
            {
                this.Tags = union(project.Tags, this.Tags);
            }
            if (project.License != null)
            {
                if (this.License == null)
                {
                    this.License = project.License;
                }
                else if (!String.IsNullOrEmpty(project.License.Type))
                {
                    this.License.Type = project.License.Type;
                }
                else if (!String.IsNullOrEmpty(project.License.Url))
                {
                    this.License.Url = project.License.Url;
                }
            }
            if (!String.IsNullOrEmpty(project.Language))
            {
                this.Language = project.Language;
            }
            if (!String.IsNullOrEmpty(project.Contributor))
            {
                this.Contributor = project.Contributor;
            }
        }

        private string[] union(string[] s1, string[] s2)
        {
            HashSet<string> joined = new HashSet<string>();
            if (s1 != null)
            {
                joined.UnionWith(s1);
            }
            if (s2 != null)
            {
                joined.UnionWith(s2);
            }
            return joined.ToArray();
        }


    }
}
