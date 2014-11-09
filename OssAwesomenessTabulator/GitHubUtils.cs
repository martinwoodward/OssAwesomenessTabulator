using Octokit;
using OssAwesomenessTabulator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OssAwesomenessTabulator
{
    public static class GitHubUtils
    {
        private static readonly string _userAgent = "OssAwesomenessTabulator";

        public static async Task<IList<Project>> GetGitHubProjects (Org org)
        {
            var github = getCient();
            var repos = await github.Repository.GetAllForOrg(org.Name);

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

        public static async Task<Project> GetGitHubProject(Project project)
        {
            // Use OckokitAPI to get data on a repo
            var github = getCient();
            var repo = await github.Repository.Get(project.GithubOrg, project.GithubRepo);

            return PopulateProjectFromRepo(project, repo);
        }

        private static Project PopulateProjectFromRepo(Project project, Repository repo)
        {
            // Fields always obtained from GitHub
            project.Created = repo.CreatedAt;
            project.Updated = repo.UpdatedAt;
            project.CommitLast = repo.PushedAt;
            project.Stars = repo.StargazersCount;
            project.Forks = repo.ForksCount;
            project.OpenIssues = repo.OpenIssuesCount;

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
            if (String.IsNullOrEmpty(project.GithubOrg))
            {
                project.GithubOrg = repo.FullName.Split('/')[0];
            }
            if (String.IsNullOrEmpty(project.GithubRepo))
            {
                project.GithubRepo = repo.Name;
            }

            // Calculate Awesomeness
            project.Awesomeness = Awesomeness.Calculate(project);

            return project;
        }


        private static GitHubClient getCient()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue(_userAgent));
            return client;
        }

    }
}
