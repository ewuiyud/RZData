using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RZData.Models;
using RZData.Services;
using System.Collections.Generic;
using RZData.Extensions;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetParentIdTest()
        {
            // 正常情况测试
            Assert.AreEqual("BA-03", ExcelDataService.GetParentId("BA-03-01"));
            Assert.AreEqual("BA-06-03", ExcelDataService.GetParentId("BA-06-03-05"));
            Assert.AreEqual("BA-06-04", ExcelDataService.GetParentId("BA-06-04-06"));
            Assert.AreEqual("A", ExcelDataService.GetParentId("A-B"));
            Assert.AreEqual("A-B", ExcelDataService.GetParentId("A-B-C"));
        }
    }
}
