using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OssAwesomenessTabulator.Data;
using OssAwesomenessTabulator;
using System.Collections.Generic;

namespace OssAwesomenessTabulatorTests
{
    [TestClass]
    public class GitHubTests
    {
        [TestMethod]
        public void TestGetGitHubProjectData()
        {
            Project actual = GitHubUtils.GetGitHubProject(new Project { GithubOrg = "SignalR", GithubRepo = "SignalR" }, null).Result;
            Assert.AreEqual<string>("SignalR", actual.Name);
            actual = GitHubUtils.GetGitHubProject(new Project { GithubOrg = "MSOpenTech", GithubRepo = "dash.js" }, null).Result;
            Assert.IsTrue(actual.IsFork);
            actual = GitHubUtils.GetGitHubProject(new Project { GithubOrg = "eclipse", GithubRepo = "jubula.core" }, null).Result;
            Assert.IsTrue(actual.IsFork);

        }

        [TestMethod]
        public void TestGetProjectsFromOrg()
        {
            IList<Project> projects = GitHubUtils.GetGitHubProjects(new Org { Name = "Microsoft"}, null).Result;
            Assert.IsNotNull(projects);
        }

    }
}
