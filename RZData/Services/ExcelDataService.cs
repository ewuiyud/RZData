using Autodesk.Revit.UI;
using Microsoft.Win32;
using OfficeOpenXml;
using RZData.Models;
using RZData.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RZData.Services
{
    public static class ExcelDataService
    {
        /// <summary>
        /// 族匹配表
        /// </summary>
        public static List<ExcelFamilyRecord> ExcelFamilyRecords = new List<ExcelFamilyRecord>();
        /// <summary>
        /// 元素编码
        /// </summary>
        public static List<TreeNode> ExcelElementCode = new List<TreeNode>();
        /// <summary>
        /// 产品编码
        /// </summary>
        public static List<TreeNode> ExcelProductCode = new List<TreeNode>();
        /// <summary>
        /// 属性字典
        /// </summary>
        public static Dictionary<string, string> ExcelPropertyDic = new Dictionary<string, string>();
        /// <summary>
        /// 材料业务规则
        /// </summary>
        public static List<ExcelMaterialBusinessRecord> ExcelMaterialBusinessRules = new List<ExcelMaterialBusinessRecord>();
        public static string LoadDataFromExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<ExcelFamilyRecord> records = new List<ExcelFamilyRecord>();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "选择Excel文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }
        public static void GetContent(string loadTemplatePath)
        {
            using (var package = new ExcelPackage(new FileInfo(loadTemplatePath)))
            {
                GetExcelFamilyRecords(package);
                GetExcelElementCode(package);
                GetExcelProductCode(package);
                GetExcelPropertyDic(package);
                GetExcelMaterialBusinessRules(package);
            }
        }

        private static void GetExcelMaterialBusinessRules(ExcelPackage package)
        {
            ExcelMaterialBusinessRules.Clear();
            var worksheet = package.Workbook.Worksheets["材料业务规则配置表"];
            int rowCount = worksheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                ExcelMaterialBusinessRecord record = new ExcelMaterialBusinessRecord();
                record.Code = worksheet.Cells[row, 1].Text;
                record.Name = worksheet.Cells[row, 2].Text;
                record.ElementName = worksheet.Cells[row, 3].Text;
                record.ProductName = worksheet.Cells[row, 4].Text;
                record.SpaceName = worksheet.Cells[row, 5].Text;
                record.ExtendRule = worksheet.Cells[row, 6].Text;
                record.ProjectCharacteristics = worksheet.Cells[row, 7].Text;
                record.UsageLocation = worksheet.Cells[row, 8].Text;

                if (!ExcelMaterialBusinessRules.Contains(record))
                {
                    ExcelMaterialBusinessRules.Add(record);
                }
            }
        }

        private static void GetExcelPropertyDic(ExcelPackage package)
        {
            ExcelPropertyDic.Clear();
            var worksheet = package.Workbook.Worksheets["属性字典"];
            int rowCount = worksheet.Dimension.Rows;
            string key = string.Empty; string value = string.Empty;
            for (int row = 2; row <= rowCount; row++)
            {
                key = worksheet.Cells[row, 1].Text;
                value = worksheet.Cells[row, 2].Text;
                if (!ExcelPropertyDic.ContainsKey(key))
                {
                    ExcelPropertyDic.Add(key, value);
                }
            }
        }

        private static void GetExcelProductCode(ExcelPackage package)
        {
            ExcelProductCode.Clear();
            var worksheet = package.Workbook.Worksheets["产品分类编码表"];
            int rowCount = worksheet.Dimension.Rows;
            string key = string.Empty; string value = string.Empty;
            for (int row = 2; row <= rowCount; row++)
            {
                key = worksheet.Cells[row, 1].Text;
                for (int col = 2; col <= 7; col++)
                {
                    value = worksheet.Cells[row, col].Text;
                    if (string.IsNullOrWhiteSpace(value))
                        continue;
                    else
                        break;
                }
                TreeNode treeNode = new TreeNode(key, value);
                var parent = ExcelProductCode.Find(x => key == x.Key.Substring(0, x.Key.Length - 3));
                if (parent != null)
                {
                    treeNode.SetParent(parent);
                }
                ExcelProductCode.Add(treeNode);
            }
        }

        private static void GetExcelElementCode(ExcelPackage package)
        {
            ExcelElementCode.Clear();
            var worksheet = package.Workbook.Worksheets["元素分类编码表"];
            int rowCount = worksheet.Dimension.Rows;
            string key = string.Empty; string value = string.Empty;
            for (int row = 2; row <= rowCount; row++)
            {
                key = worksheet.Cells[row, 1].Text;
                for (int col = 2; col <= 7; col++)
                {
                    value = worksheet.Cells[row, col].Text;
                    if (string.IsNullOrWhiteSpace(value))
                        continue;
                    else
                        break;
                }
                TreeNode treeNode = new TreeNode(key, value);
                var parent = ExcelProductCode.Find(x => x.Key == key.Substring(0, key.Length - 3));
                if (parent != null)
                {
                    treeNode.SetParent(parent);
                }
                ExcelProductCode.Add(treeNode);
            }
        }

        public static void GetExcelFamilyRecords(ExcelPackage package)
        {
            ExcelFamilyRecords = new List<ExcelFamilyRecord>();
            var worksheets = new[] { "族匹配表-装修", "族匹配表-机电", "族匹配表-结构" };
            foreach (var sheetName in worksheets)
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                if (worksheet != null)
                {
                    ReadFamilyWorksheet(worksheet, ExcelFamilyRecords);
                }
            }
        }

        private static void ReadFamilyWorksheet(ExcelWorksheet worksheet, List<ExcelFamilyRecord> records)
        {
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                ExcelFamilyRecord excelRecord = new ExcelFamilyRecord
                {
                    FamilyCategory = worksheet.Cells[row, 2].Text, // B列
                    FamilyName = worksheet.Cells[row, 3].Text, // C列
                    ExtendName = worksheet.Cells[row, 4].Text,// D列
                    ElementName = worksheet.Cells[row, 6].Text // F列
                };

                var requiredPropertie = worksheet.Cells[row, 7].Text; // G列
                var tDCName = worksheet.Cells[row, 8].Text; // H列
                excelRecord.RequiredProperties.Add(tDCName, requiredPropertie);

                var nextFamilyName = row < rowCount ? worksheet.Cells[row + 1, 2].Text : null;

                while (string.IsNullOrWhiteSpace(nextFamilyName) && row < rowCount)
                {
                    row++;
                    requiredPropertie = worksheet.Cells[row, 7].Text; // G列
                    tDCName = worksheet.Cells[row, 8].Text; // H列
                    excelRecord.RequiredProperties.Add(tDCName, requiredPropertie);
                    nextFamilyName = row < rowCount ? worksheet.Cells[row + 1, 2].Text : null;
                }

                records.Add(excelRecord);
            }
        }
        public static void ExportToExcelFromMaterialList(ObservableCollection<MaterialViewModel> materialViewModels)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("睿住数据");
                int row = 1;
                worksheet.Cells[row, 1].Value = "行号";
                worksheet.Cells[row, 2].Value = "材料名称";
                worksheet.Cells[row, 3].Value = "使用方式";
                worksheet.Cells[row, 4].Value = "项目特征";
                worksheet.Cells[row, 5].Value = "工程量";
                row++;
                foreach (var material in materialViewModels)
                {
                    worksheet.Cells[row, 1].Value = row - 1;
                    worksheet.Cells[row, 2].Value = material.MaterialName;
                    worksheet.Cells[row, 3].Value = material.UsageMethod;
                    string projectFeatures = "";
                    foreach (var item in material.ProjectFeaturesDetail)
                    {
                        projectFeatures += $"{item.Key}:{item.Value}\n";
                    }
                    worksheet.Cells[row, 4].Value = projectFeatures;
                    worksheet.Cells[row, 5].Value = material.ModelEngineeringQuantity;
                    row++;
                }

                // 保存文件
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var file = new FileInfo(saveFileDialog.FileName);
                    package.SaveAs(file);
                    TaskDialog.Show("提示", "导出成功！");
                }
            }
        }

        /// <summary>
        /// 默认输出未匹配族库表
        /// </summary>
        /// <param name="elementViewModel"></param>
        /// <param name="IsFamilyNameFalse"></param>
        internal static void ExportToExcelFromElement(ElementViewModel elementViewModel, bool IsFamilyNameFalse = true)
        {
            // 从 Resources 中加载模板文件
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = IsFamilyNameFalse ? "RZData.Resources.Templates.族匹配校验模板.xlsx" : "RZData.Resources.Templates.参数项错误表模板xlsx.xlsx";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // 假设模板文件中有一个工作表
                    List<(string FamilyCategory, string FamilyName, string ExtendName, string Parameters)> list =
                        new List<(string FamilyCategory, string FamilyName, string ExtendName, string Parameters)>();
                    foreach (var revitSolidElement in elementViewModel.RevitSolidElements)
                    {
                        var parameters = revitSolidElement.Parameters.FindAll(a => a.Value == "缺失").ToList();
                        string strPar = "";
                        foreach (var paramter in parameters)
                        {
                            strPar += paramter.Name + "\n";
                        }
                        list.Add((revitSolidElement.FamilyCategory, revitSolidElement.FamilyName, revitSolidElement.ExtendName, strPar));
                        //list去重
                        list = list.Distinct().ToList();
                    }
                    int row = 2;
                    foreach (var item in list)
                    {
                        worksheet.Cells[row, 1].Value = item.FamilyCategory;
                        worksheet.Cells[row, 2].Value = item.FamilyName;
                        worksheet.Cells[row, 3].Value = item.ExtendName;
                        if (!IsFamilyNameFalse)
                            worksheet.Cells[row, 4].Value = item.Parameters;
                        row++;
                    }

                    // 保存文件
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel files (*.xlsx)|*.xlsx",
                        FilterIndex = 2,
                        RestoreDirectory = true
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var file = new FileInfo(saveFileDialog.FileName);
                        package.SaveAs(file);
                        TaskDialog.Show("提示", "导出成功！");
                    }
                }
            }
        }
    }
}
