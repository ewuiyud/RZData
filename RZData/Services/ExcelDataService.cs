using Autodesk.Revit.UI;
using Microsoft.Win32;
using OfficeOpenXml;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.IO;

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

        public static void ExportToExcel(DataElement dataElementData)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("睿住数据");
                int row = 1;
                worksheet.Cells[row, 1].Value = "族类别";
                worksheet.Cells[row, 2].Value = "族";
                worksheet.Cells[row, 3].Value = "补充属性";
                worksheet.Cells[row, 4].Value = "ID";
                row++;
                foreach (var family in dataElementData.FamilyCategories)
                {
                    foreach (var familyType in family.Families)
                    {
                        foreach (var familyExtend in familyType.FamilyExtends)
                        {
                            foreach (var dataInstance in familyExtend.DataInstances)
                            {
                                worksheet.Cells[row, 1].Value = family.Name;
                                worksheet.Cells[row, 2].Value = familyType.Name;
                                worksheet.Cells[row, 3].Value = familyExtend.Name;
                                worksheet.Cells[row, 4].Value = dataInstance.Element.Id;
                                row++;
                            }
                        }
                    }
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
