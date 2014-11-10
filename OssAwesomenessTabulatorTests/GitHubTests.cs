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
            Project lookup = new Project { GithubOrg = "SignalR", GithubRepo = "SignalR" };
            Project actual = GitHubUtils.GetGitHubProject(lookup, null).Result;
            Assert.AreEqual<string>("SignalR", actual.Name);
        }

        [TestMethod]
        public void TestGetProjectsFromOrg()
        {
            IList<Project> projects = GitHubUtils.GetGitHubProjects(new Org { Name = "Microsoft"}, null).Result;
            Assert.IsNotNull(projects);
        }

    }
}
