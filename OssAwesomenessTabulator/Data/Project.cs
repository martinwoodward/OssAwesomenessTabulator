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
    }
}
