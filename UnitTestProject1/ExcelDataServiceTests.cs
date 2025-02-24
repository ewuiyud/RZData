using Microsoft.VisualStudio.TestTools.UnitTesting;
using RZData.Models;
using RZData.Services;
using RZData.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace UnitTestProject1
{
    //[TestClass]
    //public class ExcelDataServiceTests
    //{
    //    [TestMethod]
    //    public void LoadDataFromExcel_ShouldReturnFilePath_WhenFileSelected()
    //    {
    //        // Arrange
    //        // Mock OpenFileDialog behavior here if necessary

    //        // Act
    //        var result = ExcelDataService.LoadDataFromExcel();

    //        // Assert
    //        Assert.IsNotNull(result);
    //        Assert.IsTrue(File.Exists(result));
    //    }

    //    [TestMethod]
    //    public void GetContent_ShouldPopulateDataStructures_WhenValidExcelFileProvided()
    //    {
    //        // Arrange
    //        string filePath = "path_to_valid_excel_file.xlsx";
    //        ExcelDataService.ExcelFamilyRecords.Clear();
    //        ExcelDataService.ExcelElementCode.Clear();
    //        ExcelDataService.ExcelProductCode.Clear();
    //        ExcelDataService.ExcelPropertyDic.Clear();
    //        ExcelDataService.ExcelMaterialBusinessRules.Clear();

    //        // Act
    //        ExcelDataService.GetContent(filePath);

    //        // Assert
    //        Assert.IsTrue(ExcelDataService.ExcelFamilyRecords.Count > 0);
    //        Assert.IsTrue(ExcelDataService.ExcelElementCode.Count > 0);
    //        Assert.IsTrue(ExcelDataService.ExcelProductCode.Count > 0);
    //        Assert.IsTrue(ExcelDataService.ExcelPropertyDic.Count > 0);
    //        Assert.IsTrue(ExcelDataService.ExcelMaterialBusinessRules.Count > 0);
    //    }

    //    [TestMethod]
    //    public void ExportToExcelFromMaterialList_ShouldCreateExcelFile_WhenCalled()
    //    {
    //        // Arrange
    //        var materialViewModels = new ObservableCollection<MaterialViewModel>
    //        {
    //            new MaterialViewModel
    //            {
    //                MaterialName = "Material1",
    //                UsageMethod = "Usage1",
    //                ProjectFeaturesDetail = new Dictionary<string, string> { { "Feature1", "Value1" } },
    //                ModelEngineeringQuantity = 10.0
    //            }
    //        };

    //        // Act
    //        ExcelDataService.ExportToExcelFromMaterialList(materialViewModels);

    //        // Assert
    //        // Verify that the file was created and contains the expected data
    //    }
    //}
}
