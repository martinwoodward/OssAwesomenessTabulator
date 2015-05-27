using Octokit;
using OssAwesomenessTabulator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OssAwesomenessTabulator
{
    public class GitHubUtils
    {
        private GitHubClient _client;
        private Credentials _creds;
        private static readonly string _userAgent = "OssAwesomenessTabulator";
        private static readonly char[] _nameDelims = new char[] { '.', '_','-' };   

        public GitHubUtils(Credentials creds)
        {
            _creds = creds;
        }

        public GitHubClient GitHub
        {
            get
            {
                if (_client == null)
                {
                    _client = new GitHubClient(new ProductHeaderValue(_userAgent));
                    if (_creds != null)
                    {
                        _client.Credentials = _creds;
                    }
                }
                return _client;
            }
        }

        public async Task<IList<Project>> GetGitHubProjects(Org org)
        {
            var repos = await GitHub.Repository.GetAllForOrg(org.Name);

            List<Project> projects = new List<Project>(repos.Count);

            foreach(var repo in repos)
            {
                if (!repo.Fork)
                {
                    // Ignore GitHub projects that are forks of other GitHub projects
                    // as they are usually for contributing back stuff, project doesn't
                    // really "belong" to the org in terms of awesomeness.
                    Project project = PopulateProjectFromRepo(new Project(), repo);
                    if (!String.IsNullOrEmpty(org.Contributor))
                    {
                        project.Contributor = org.Contributor;
                    }
                    projects.Add(project);
                }
            }

            return projects;
        }

        public async Task<Project> GetGitHubProject(Project project)
        {
            // Use OckokitAPI to get data on a repo
            var repo = await GitHub.Repository.Get(project.GithubOrg, project.GithubRepo);

            return PopulateProjectFromRepo(project, repo);
        }

        private Project PopulateProjectFromRepo(Project project, Repository repo)
        {
            // Get GitHub org and repo if we haven't allready
            if (String.IsNullOrEmpty(project.GithubOrg))
            {
                project.GithubOrg = repo.FullName.Split('/')[0];
            }
            if (String.IsNullOrEmpty(project.GithubRepo))
            {
                project.GithubRepo = repo.Name;
            }
            
            // Fields always obtained from GitHub
            project.Created = repo.CreatedAt;
            project.Updated = repo.UpdatedAt;
            project.CommitLast = repo.PushedAt;
            project.Stars = repo.StargazersCount;
            project.Forks = repo.ForksCount;
            project.OpenIssues = repo.OpenIssuesCount;

            if (project.CommitLast != null &&
                 (project.CommitLast != project.Created) &&
                  project.Stars > 0)
            {
                // There is currently an issue in the Octokit API when looking 
                // for contributors on a repo that doesn't have anything in it. 
                // Only bother to look if the repo looks interesting.
                project.Contributors = getContribCount(project.GithubOrg, project.GithubRepo).Result;

            }

            // Fields defaulted from GitHub but could have overrides specified
            if (String.IsNullOrEmpty(project.Name))
            {
                project.Name = repo.Name;
            }
            if (String.IsNullOrEmpty(project.Url))
            {
                // project.Url = !String.IsNullOrEmpty(repo.Homepage) ? repo.Homepage : repo.HtmlUrl;
                project.Url = repo.HtmlUrl;
            }
            if (String.IsNullOrEmpty(project.Description))
            {
                project.Description = repo.Description;
            }
            if (String.IsNullOrEmpty(project.Language))
            {
                project.Language = repo.Language;
            }
            if (!project.Fork)
            {
                if (repo.Fork)
                {
                    project.Fork = true;
                    project.ForkedFrom = repo.Parent.FullName;
                    project.ForkedFromUrl = repo.Parent.HtmlUrl;
                }
                if (!String.IsNullOrEmpty(repo.MirrorUrl))
                {
                    project.Fork = true;
                    project.ForkedFromUrl = repo.MirrorUrl;
                }
            }

            project.Tags = getTags(project, repo);
            
            // Calculate Awesomeness
            project.Awesomeness = Awesomeness.Calculate(project);
            return project;
        }

        private string[] getTags(Project project, Repository repo)
        {
            // Make tags case insensitive
            HashSet<string> tags = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
 
            // Pass through existing tags
            if (project.Tags != null)
            {
                tags.UnionWith(project.Tags);
            }

            // Calulcate additional tags
            if (!String.IsNullOrEmpty(repo.Language))
            {
                tags.Add(repo.Language);
            }
            if (!String.IsNullOrEmpty(project.Contributor))
            {
                tags.Add(project.Contributor);
            }
            if (!String.IsNullOrEmpty(project.GithubOrg))
            {
                tags.Add(project.GithubOrg);
            }

            // Pull out the begginging part of a repo name when they have MyThing-something or MyThing.Something
            if (project.Name.IndexOfAny(_nameDelims) > 1)
            {
                tags.Add(project.Name.Substring(0, project.Name.IndexOfAny(_nameDelims)));
            }

            if (tags.Count == 0)
            {
                return null;
            }

            // Loop over tags and convert to lower case.
            string[] calculatedTags = new string[tags.Count];
            for (int i = 0; i < calculatedTags.Length; i++)
            {
                calculatedTags[i] = tags.ElementAt<string>(i).ToLowerInvariant();
            }

            return calculatedTags;
        }

        public bool isHealthy()
        {
            bool up = true;
            try
            {
                using (var web = new WebClient())
                {
                    string status = JsonConvert
                        .DeserializeObject<GitHubStatus>(
                            web.DownloadString("https://status.github.com/api/status.json"))
                        .Status;
                    if (status.ToLower() != "good")
                    {
                        // Status can be good (green), minor (yellow), or major (red)
                        up = false;
                        System.Diagnostics.Trace.TraceError("GitHub site reporting {0} issues. See https://status.github.com/", status);
                    }
                }
            }
            catch (Exception ex)
            {
                up = false;
                System.Diagnostics.Trace.TraceError("Exception checking GitHub status: {0}", ex.StackTrace);
            }

            return up;
        }

        private async Task<int> getContribCount(string org, string repo)
        {
            int contributors = 0;
            try
            {
                IReadOnlyList<User> users = await GitHub.Repository.GetAllContributors(org, repo);
                if (users != null)
                {
                    contributors = users.Count();
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Exception detected getting contributors for \"{0}/{1}\": {2}", org, repo, ex.StackTrace);
            }
            return contributors;
        }

        class GitHubStatus
        {
            public string Status {get; set;}
            public DateTimeOffset LastUpdated {get; set;}
        }

    }
}
