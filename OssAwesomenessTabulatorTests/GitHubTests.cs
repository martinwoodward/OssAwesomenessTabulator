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
        private GitHubUtils github = new GitHubUtils(null);

        [TestMethod]
        public void TestGetGitHubProjectData()
        {
            Project actual = github.GetGitHubProject(new Project { GithubOrg = "SignalR", GithubRepo = "SignalR" }).Result;
            Assert.AreEqual<string>("SignalR", actual.Name);
            actual = github.GetGitHubProject(new Project { GithubOrg = "MSOpenTech", GithubRepo = "dash.js" }).Result;
            Assert.IsTrue(actual.Fork);
            actual = github.GetGitHubProject(new Project { GithubOrg = "eclipse", GithubRepo = "jubula.core" }).Result;
            Assert.IsTrue(actual.Fork);
        }

        [TestMethod]
        public void TestGetProjectsFromOrg()
        {
            IList<Project> projects = github.GetGitHubProjects(new Org { Name = "Microsoft"}).Result;
            Assert.IsNotNull(projects);
        }

        [TestMethod]
        public void CheckGitHubStatus()
        {
            Assert.IsTrue(github.isHealthy());
        }

        [TestMethod]
        public void CalculateTags()
        {
            string[] actual = github.GetGitHubProject(new Project { GithubOrg = "JackFullerton", GithubRepo = "JackFullerton.github.io", Tags = new string[] { "jack", "dnf", "oss" } }).Result.Tags;
            string[] expected = new string[] { "oss", "dnf", "jack" };

            CollectionAssert.AreEquivalent(expected, actual);
        }


    }
}
