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
                    projects.Add(PopulateProjectFromRepo(new Project(), repo));
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

            project.Contributors = getContribCount(project.GithubOrg, project.GithubRepo).Result;

            // Fields defaulted from GitHub but could have overrides specified
            if (String.IsNullOrEmpty(project.Name))
            {
                project.Name = repo.Name;
            }
            if (String.IsNullOrEmpty(project.Url))
            {
                project.Url = !String.IsNullOrEmpty(repo.Homepage) ? repo.Homepage : repo.HtmlUrl;
            }
            if (String.IsNullOrEmpty(project.Description))
            {
                project.Description = repo.Description;
            }
            if (String.IsNullOrEmpty(project.Language))
            {
                project.Language = repo.Language;
            }
            if (!project.IsFork)
            {
                project.IsFork = repo.Fork || (!String.IsNullOrEmpty(repo.MirrorUrl));
            }

            // Calculate Awesomeness
            project.Awesomeness = Awesomeness.Calculate(project);
            return project;
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
                contributors = users.Count();
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
