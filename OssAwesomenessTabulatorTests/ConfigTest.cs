using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OssAwesomenessTabulator;
using System.Collections.Generic;
using OssAwesomenessTabulator.Data;

namespace OssAwesomenessTabulatorTests
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void TestLoad()
        {
            Config config = Config.LoadFromWeb("https://raw.githubusercontent.com/Microsoft/microsoft.github.io/master/data");

            Assert.IsNotNull(config.GetOrgs());
            Assert.IsNotNull(config.GetProjects());
        }
    }
}
