using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OssAwesomenessTabulator;
using System.IO;
using System.Text;

namespace OssAwesomenessTabulatorTests
{
    [TestClass]
    public class FunctionTest
    {
        [TestMethod]
        public void TestWriteTop()
        {
            string actual;
            using (var output = new MemoryStream())
            {
                Functions.WriteTop(output);
                actual = Encoding.UTF8.GetString(output.ToArray());
            }
            Assert.AreEqual<string>("top",actual);
        }



    }
}
