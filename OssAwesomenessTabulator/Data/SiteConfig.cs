using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OssAwesomenessTabulator.Data
{
    public class SiteConfig
    {
        public Contributors[] Contributors { get; set; }
        public Project[] Projects { get; set; }
        public Contributors[] GithubOrgs { get; set; }
    }
}
