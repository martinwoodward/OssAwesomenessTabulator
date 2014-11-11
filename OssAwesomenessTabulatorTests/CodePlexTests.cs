using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OssAwesomenessTabulator;
using OssAwesomenessTabulator.Data;

namespace OssAwesomenessTabulatorTests
{
    [TestClass]
    public class CodePlexTests
    {
        private CodePlexUtils codeplex = new CodePlexUtils();

        [TestMethod]
        public void TestGetProject()
        {
            Project actual = codeplex.GetProject(new Project { CodePlexProject = "http://pytools.codeplex.com/" });
            Assert.AreEqual("Python Tools for Visual Studio", actual.Name);
            actual = codeplex.GetProject(new Project { CodePlexProject = "http://magick.codeplex.com/" });
            Assert.AreEqual("Magick.NET", actual.Name);
            actual = codeplex.GetProject(new Project { CodePlexProject = "http://visualhg.codeplex.com/" });
            Assert.AreEqual("VisualHG", actual.Name);
            actual = codeplex.GetProject(new Project { CodePlexProject = "http://mwtest01.codeplex.com/" });
            Assert.AreEqual("mwtest01", actual.Name);
            actual = codeplex.GetProject(new Project { CodePlexProject = "http://tfsapi.codeplex.com/" });
            Assert.AreEqual("TFS API", actual.Name);
        }

        [TestMethod]
        public void TestGetUser()
        {
            // Note, takes a LONG time for "Microsoft"
            var actual = codeplex.GetProjects("MSOpenTech");
            Assert.IsTrue(actual.Length > 0);
        }

        [TestMethod]
        public void TestStatus()
        {
            Assert.IsTrue(codeplex.IsHealthy());
        }

    }
}
