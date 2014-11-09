using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OssAwesomenessTabulator.Data;
using OssAwesomenessTabulator;

namespace OssAwesomenessTabulatorTests
{
    [TestClass]
    public class AwesomenessTests
    {
        [TestMethod]
        public void TestAwesomeness()
        {
            Project p = new Project
            {
                Created = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(300)),
                CommitLast = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(31)),
                Stars = 10
            };
            Assert.AreEqual(53, Awesomeness.Calculate(p));
        }

        [TestMethod]
        public void TestNothingDone()
        {
            Project p = new Project
            {
                Created = DateTimeOffset.Now,
                Stars = 1000
            };
            Assert.AreEqual(0, Awesomeness.Calculate(p));
        }

        [TestMethod]
        public void TestMoreAwesomeThanTheHoff()
        {
            Project p = new Project
            {
                Created = DateTimeOffset.Now,
                CommitLast = DateTimeOffset.Now,
                Stars = 1000000
            };
            Assert.AreEqual(32767, Awesomeness.Calculate(p));
        }

    }
}
