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
    public class RevitElementServiceTests
    {
        private Mock<UIDocument> _mockUiDocument;
        private Mock<Document> _mockDocument;
        private Mock<FilteredElementCollector> _mockCollector;
        private RevitElementService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            //_mockUiDocument = new Mock<UIDocument>();
            //_mockDocument = new Mock<Document>();
            //_mockCollector = new Mock<FilteredElementCollector>(_mockDocument.Object);
            _service = new RevitElementService();
        }

        [TestMethod]
        public void LoadAllRevitElements_ShouldReturnCorrectElements()
        {
            // Arrange
            var elements = new List<Element>
            {
                CreateMockElement("MIC_Family1", "Category1"),
                CreateMockElement("System_Family2", "Category2"),
                CreateMockElement("MIC_Family3", "Category3")
            };
            _mockCollector.Setup(c => c.WhereElementIsNotElementType()).Returns(_mockCollector.Object);
            _mockCollector.Setup(c => c.ToElements()).Returns(elements);
            _mockUiDocument.Setup(d => d.Document).Returns(_mockDocument.Object);

            // Act
            var result = _service.LoadAllRevitElements(_mockUiDocument.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void ProcessNonFamilyInstance_ShouldSetCorrectProperties()
        {
            // Arrange
            var element = CreateMockElement("System_Family2", "Category2");
            var revitSolidElement = new RevitSolidElement(element);
            var systemFamilyDictionary = new List<ExcelFamilyRecord>
            {
                new ExcelFamilyRecord { FamilyName = "System_Family2", FamilyCategory = "Category2", ExtendName = "ExtendName2" }
            };

            // Act
            _service.ProcessNonFamilyInstance(systemFamilyDictionary, _mockDocument.Object, element, revitSolidElement);

            // Assert
            Assert.IsTrue(revitSolidElement.IsNameCorrect);
        }

        [TestMethod]
        public void ProcessFamilyInstance_ShouldSetCorrectProperties()
        {
            // Arrange
            var element = CreateMockElement("MIC_Family1", "Category1");
            var revitSolidElement = new RevitSolidElement(element, RevitElementFamilyType.LoadFamilyElement);
            var loadableFamilyDictionary = new List<ExcelFamilyRecord>
            {
                new ExcelFamilyRecord { FamilyName = "MIC_Family1", FamilyCategory = "Category1", ElementName = "Element1" }
            };

            // Act
            _service.ProcessFamilyInstance(loadableFamilyDictionary, _mockDocument.Object, element, revitSolidElement);

            // Assert
            Assert.IsTrue(revitSolidElement.IsNameCorrect);
            Assert.AreEqual("Element1", revitSolidElement.ElementName);
        }

        [TestMethod]
        public void CheckParameters_ShouldReturnCorrectResult()
        {
            // Arrange
            var element = CreateMockElement("MIC_Family1", "Category1");
            var revitSolidElement = new RevitSolidElement(element);
            var excelRecord = new ExcelFamilyRecord
            {
                FamilyName = "MIC_Family1",
                FamilyCategory = "Category1",
                RequiredProperties = new Dictionary<string, string> { { "Property1", "Value1" } }
            };

            // Act
            var result = _service.CheckParameters(excelRecord, _mockDocument.Object, element, revitSolidElement);

            // Assert
            Assert.IsTrue(result);
        }

        private Element CreateMockElement(string familyName, string familyCategory)
        {
            var mockElement = new Mock<Element>();
            mockElement.Setup(e => e.GetFamilyName()).Returns(familyName);
            mockElement.Setup(e => e.GetFamilyCategory()).Returns(familyCategory);
            return mockElement.Object;
        }
    }
}
