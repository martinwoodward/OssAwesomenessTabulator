using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OssAwesomenessTabulator;
using System.IO;
using System.Text;
using OssAwesomenessTabulator.Data;
using System.Linq;

namespace OssAwesomenessTabulatorTests
{
    [TestClass]
    public class FunctionTest
    {
        [TestMethod]
        public void TestWriteFiles()
        {
            string actual;

            OssData data = new OssData();
            data.AddProject(new Project
            {
                Awesomeness = 1,
                Name = "ASP.NET SignalR",
                Url = "http://signalr.net/",
                Description = "[ASP.NET SignalR](http://signalr.net/) is a library for ASP.NET developers that makes it incredibly simple to add real-time web functionality to your applications.",
                License = new License { Type="Apache 2.0" },
                Keywords = new string[]{"SignalR","ASP.NET"},
                Tags = new string[]{".NET Foundation","ASP.NET","Microsoft","C#"},
                Contributor = "Microsoft",
                Language = "C#",
                GithubOrg = "SignalR",
                GithubRepo = "SignalR",
                Created = DateTime.Parse("2011-07-22 13:26"),
                CommitLast = DateTime.Parse("2014-10-10 17:19"),
                Updated = DateTime.Parse("2014-11-06 22:12"),
                Stars = 4582,
                Forks = 1302,
                Contributors = 59
            });
            data.AddProject(new Project
            {
                Awesomeness = 3,
                Name = "ASP.NET MVC, Web API and Web Pages (Razor)",
                Url = "https://aspnetwebstack.codeplex.com/",
                Description = "[ASP.NET](http://asp.net) is a free web framework for building great web sites and applications. The ASP.NET web stack includes ASP.NET MVC 4.0, 5.0, Web API 1.0, 2.0, and Web Pages, 2,0, 3.0 source code. These products are actively developed by the ASP.NET team in collaboration with a community of open source developers. Together we are dedicated to creating the best possible platform for web development.",
                License = new License { Type="Apache 2.0" },
                Keywords = new string[]{"Razor","ASP.NET","Web Pages","IIS","MVC"},
                Tags = new string[]{".NET Foundation","ASP.NET","Microsoft","C#"},
                Contributor = "Microsoft",
                Language = "C#",
                CodePlexProject = "https://aspnetwebstack.codeplex.com/",
                Created = DateTime.Parse("2011-07-22 13:26"),
                CommitLast = DateTime.Parse("2014-10-10 17:19"),
                Updated = DateTime.Parse("2014-11-06 22:12"),
                Stars = 2752,
                Forks = 385,
                Contributors = 37
            });
            data.AddProject(new Project
            {
                Awesomeness = 2,
                Name = "Microsoft Azure SDK for .NET",
                Url = "https://github.com/Azure/azure-sdk-for-net",
                Description = "The [Microsoft Azure SDK for .NET](http://azure.microsoft.com/en-us/develop/net/) allows you to build applications that take advantage of scalable cloud computing resources.",
                License = new License { Type="Apache 2.0" },
                Tags = new string[]{".NET Foundation","Azure","Microsoft","C#","SDK"},
                Contributor = "Microsoft",
                Language = "C#",
                GithubOrg = "Azure",
                GithubRepo = "azure-sdk-for-net",
                Created = DateTime.Parse("2011-12-09 19:10"),
                CommitLast = DateTime.Parse("2014-11-06 22:54"),
                Updated = DateTime.Parse("2014-11-06 22:54"),
                Stars = 555,
                Forks = 258,
                Contributors = 53
            });
            data.AddProject(new Project
            {
                Awesomeness = 4,
                Name = "Couchbase Lite for .NET",
                Url = "https://github.com/couchbaselabs/couchbase-lite-net",
                Description = "This project is a port of the [Couchbase Lite](http://developer.couchbase.com/mobile/) portable Java codebase, ported to C#. Couchbase Lite is a fully functional, on-device, lightweight, native, embedded JSON database. With Couchbase Lite, you have the full power of a Couchbase database locally on the device. You can create, update, delete, query, sync and much, much more.",
                License = new License { Type = "MIT", Url = "https://github.com/couchbaselabs/couchbase-lite-net/blob/master/LICENSE" },
                Keywords = new string[] { "DB", "JSonDB", "Json"},
                Tags = new string[] { ".NET Foundation", "Couchbase", "C#" },
                Contributor = "Couchbase",
                Language = "C#",
                GithubOrg = "couchbaselabs",
                GithubRepo = "couchbase-lite-net",
                Created = DateTime.Parse("2013-10-14 09:18"),
                CommitLast = DateTime.Parse("2014-11-06 00:12"),
                Updated = DateTime.Parse("2014-11-06 00:12"),
                Stars = 76,
                Forks = 21,
                Contributors = 11
            });
           
            using (var output = new MemoryStream())
            {
                Functions.WriteTop(output, data);
                actual = Encoding.UTF8.GetString(output.ToArray());
            }

            File.WriteAllText("c:\\temp\\project_example.json", actual);

            Assert.AreEqual<string>("top","top");
        }


        [TestMethod]
        public void WriteMicrosoftOrgFile()
        {
            Org org = new Org { Name = "Microsoft" };

            OssData data = new OssData();
            data.AddProjects(GitHubUtils.GetGitHubProjects(org).Result.ToArray());

            String json;
            using (var output = new MemoryStream())
            {
                Functions.Write(output, data);
                json = Encoding.UTF8.GetString(output.ToArray());
            }
            File.WriteAllText("c:\\temp\\microsoft.json", json);
            using (var output = new MemoryStream())
            {
                Functions.WriteTop(output, data);
                json = Encoding.UTF8.GetString(output.ToArray());
            }
            File.WriteAllText("c:\\temp\\microsoft_top.json", json);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void WriteProjectsFile()
        {
            OssData data = Functions.GetData("https://raw.githubusercontent.com/Microsoft/microsoft.github.io/master/data/organization.json");

            String json;
            using (var output = new MemoryStream())
            {
                Functions.Write(output, data);
                json = Encoding.UTF8.GetString(output.ToArray());
            }
            File.WriteAllText("c:\\temp\\projects.json", json);
            using (var output = new MemoryStream())
            {
                Functions.WriteTop(output, data);
                json = Encoding.UTF8.GetString(output.ToArray());
            }
            File.WriteAllText("c:\\temp\\projects_top.json", json);
            Assert.IsTrue(true);
        }


    }
}
