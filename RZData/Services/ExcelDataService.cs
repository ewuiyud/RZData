using Autodesk.Revit.UI;
using Microsoft.Win32;
using Models;
using OfficeOpenXml;
using RZData.Models;
using RZData.Tools;
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
        public static List<ExcelFamilyNameModel> ExcelFamilyRecords = new List<ExcelFamilyNameModel>();
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
        public static List<ExcelMaterialBusinessModel> ExcelMaterialBusinessRules = new List<ExcelMaterialBusinessModel>();
        /// <summary>
        /// 产品物料库
        /// </summary>
        public static List<ExcelProductMaterialLibraryModel> ExcelProductMaterialLibraryModels = new List<ExcelProductMaterialLibraryModel>();
        public static string LoadDataFromExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
                GetExcelProductMaterialLibrary(package);
            }
        }

        /// <summary>
        /// 获取产品物料库
        /// </summary>
        /// <param name="package"></param>
        private static void GetExcelProductMaterialLibrary(ExcelPackage package)
        {
            ExcelProductMaterialLibraryModels.Clear();
            var worksheet = package.Workbook.Worksheets["产品物料库"];
            int rowCount = worksheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                ExcelProductMaterialLibraryModel record = new ExcelProductMaterialLibraryModel
                {
                    Name = worksheet.Cells[row, 1].Text,
                    Description = worksheet.Cells[row, 2].Text,
                    Brand = worksheet.Cells[row, 3].Text,
                    SpecificationAttributes = worksheet.Cells[row, 4].Text,
                    SpecificationAttributesDetail = ExplainSpecificationAttributes(worksheet.Cells[row, 4].Text),
                    ProductName = worksheet.Cells[row, 5].Text,
                    SerialNumber = worksheet.Cells[row, 6].Text
                };
                if (!ExcelProductMaterialLibraryModels.Contains(record))
                {
                    ExcelProductMaterialLibraryModels.Add(record);
                }
            }
        }

        /// <summary>
        /// 将规格属性字符串解释为规格属性字典
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static Dictionary<string, string> ExplainSpecificationAttributes(string input)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(input))
                return result;

            string[] lines = input.Split(
                new[] { Environment.NewLine, "\n", "\r" },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                int separatorIndex = line.IndexOf('=');
                if (separatorIndex > 0 && separatorIndex < line.Length - 1)
                {
                    string key = line.Substring(0, separatorIndex).Trim();
                    string value = line.Substring(separatorIndex + 1).Trim();
                    result[key] = value;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取材料业务规则配置表
        /// </summary>
        /// <param name="package"></param>
        private static void GetExcelMaterialBusinessRules(ExcelPackage package)
        {
            ExcelMaterialBusinessRules.Clear();
            var worksheet = package.Workbook.Worksheets["材料业务规则配置表"];
            int rowCount = worksheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                ExcelMaterialBusinessModel record = new ExcelMaterialBusinessModel
                {
                    ID = worksheet.Cells[row, 1].Text,
                    Name = worksheet.Cells[row, 2].Text,
                    ElementName = worksheet.Cells[row, 3].Text,
                    ProductName = worksheet.Cells[row, 4].Text,
                    SpaceName = worksheet.Cells[row, 5].Text,
                    ExtendRule = worksheet.Cells[row, 6].Text,
                    ProjectCharacteristics = worksheet.Cells[row, 7].Text,
                    UsageLocation = worksheet.Cells[row, 8].Text,
                    ModelEngineeringQuantity = worksheet.Cells[row, 9].Text,
                    ModelEngineeringUnit = worksheet.Cells[row, 10].Text,
                    MaterialQuantityHasLibrary = worksheet.Cells[row, 11].Text,
                    MaterialQuantityNoLibrary = worksheet.Cells[row, 12].Text,
                    MaterialUnit = worksheet.Cells[row, 13].Text,
                    LossValue = worksheet.Cells[row, 14].Text,
                    MaterialProcurementQuantityHasLibrary = worksheet.Cells[row, 15].Text,
                    MaterialProcurementQuantityNoLibrary = worksheet.Cells[row, 16].Text,
                    ProcurementUnit = worksheet.Cells[row, 17].Text,
                };

                if (!ExcelMaterialBusinessRules.Contains(record))
                {
                    ExcelMaterialBusinessRules.Add(record);
                }
            }
        }

        /// <summary>
        /// 获取属性字典表
        /// </summary>
        /// <param name="package"></param>
        private static void GetExcelPropertyDic(ExcelPackage package)
        {
            ExcelPropertyDic.Clear();
            var worksheet = package.Workbook.Worksheets["属性字典"];
            int rowCount = worksheet.Dimension.Rows;
            string key; string value;
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

        /// <summary>
        /// 获取产品分类编码表
        /// </summary>
        /// <param name="package"></param>
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

        /// <summary>
        /// 获取元素分类编码表
        /// </summary>
        /// <param name="package"></param>
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
                var parent = ExcelElementCode.Find(x => x.Key == key.Substring(0, key.Length - 3));
                if (parent != null)
                {
                    treeNode.SetParent(parent);
                }
                ExcelElementCode.Add(treeNode);
            }
        }

        /// <summary>
        /// 获取族匹配表
        /// </summary>
        /// <param name="package"></param>
        public static void GetExcelFamilyRecords(ExcelPackage package)
        {
            ExcelFamilyRecords = new List<ExcelFamilyNameModel>();
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

        private static void ReadFamilyWorksheet(ExcelWorksheet worksheet, List<ExcelFamilyNameModel> records)
        {
            int rowCount = worksheet.Dimension.Rows;
            string result = "";
            for (int row = 2; row <= rowCount; row++)
            {
                ExcelFamilyNameModel excelRecord = new ExcelFamilyNameModel
                {
                    FamilyCategory = worksheet.Cells[row, 2].Text, // B列
                    FamilyName = worksheet.Cells[row, 3].Text, // C列
                    ExtendName = worksheet.Cells[row, 4].Text,// D列
                    ElementName = worksheet.Cells[row, 6].Text // F列
                };
                var excelParameterModel = FillParameters(worksheet, row, excelRecord);
                if (string.IsNullOrEmpty(excelParameterModel.TDCName))
                {
                    continue;
                }
                excelRecord.RequiredProperties.Add(excelParameterModel);

                var nextFamilyName = row < rowCount ? worksheet.Cells[row + 1, 2].Text : null;

                while (string.IsNullOrWhiteSpace(nextFamilyName) && row < rowCount)
                {
                    row++;
                    excelParameterModel = FillParameters(worksheet, row, excelRecord);
                    if (string.IsNullOrEmpty(excelParameterModel.TDCName))
                    {
                        continue;
                    }
                    excelRecord.RequiredProperties.Add(excelParameterModel);
                    nextFamilyName = row < rowCount ? worksheet.Cells[row + 1, 2].Text : null;
                }
                if (!excelRecord.FamilyName.StartsWith(ConstString.ParameterPrex))
                {
                    if (!excelRecord.ExtendName.StartsWith(ConstString.ExtendNamePrefix))
                    {
                        result += $"第{row}行数据，{excelRecord.FamilyCategory}-{excelRecord.FamilyName}-{excelRecord.ExtendName}的内容不规范，未能录入该条数据。\n";
                        continue;
                    }
                }
                records.Add(excelRecord);
            }
            if (!string.IsNullOrEmpty(result))
            {
                TaskDialog.Show("载入结果", $"表格{worksheet}中:\n" + result);
            }
        }

        private static ExcelParameterModel FillParameters(ExcelWorksheet worksheet, int row, ExcelFamilyNameModel excelRecord)
        {
            ExcelParameterModel excelParameterModel = new ExcelParameterModel
            {
                Name = worksheet.Cells[row, 7].Text, // G列
                TDCName = worksheet.Cells[row, 8].Text, // H列
                ValueEnumString = worksheet.Cells[row, 9].Text, // I列
                StandardValue = worksheet.Cells[row, 10].Text, // J列
                Unit = worksheet.Cells[row, 11].Text, // K列
                IsShowed = string.IsNullOrEmpty(worksheet.Cells[row, 12].Text), // L列
                Reference = worksheet.Cells[row, 13].Text // M列
            };
            return excelParameterModel;
        }

        public static void ExportToExcelFromMaterialList(ObservableCollection<MaterialViewModel> materialViewModels)
        {
            // 从 Resources 中加载模板文件
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "RZData.Resources.Templates.项目材料清单模板.xlsx";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    // 分组逻辑
                    var groupedElements = materialViewModels.GroupBy(m => GetParentId(m.ID))
                                                  .ToDictionary(g => g.Key, g => g.ToList());
                    int row = 4; int index = 1;
                    foreach (var group in groupedElements)
                    {
                        worksheet.Cells[row, 1].Value = group.Key + " " +
                            ExcelDataService.ExcelMaterialBusinessRules.Find(a => a.ID == group.Key).Name;
                        // 合并指定范围的单元格
                        string range = $"A{row}:J{row}";
                        worksheet.Cells[range].Merge = true;
                        worksheet.Cells[range].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        row++;
                        foreach (var material in group.Value)
                        {
                            string parentID = GetParentId(material.ID);
                            worksheet.Cells[row, 1].Value = index;
                            index++;
                            worksheet.Cells[row, 2].Value = material.MaterialName;
                            worksheet.Cells[row, 3].Value = material.UsageMethod;
                            worksheet.Cells[row, 4].Value = material.ProjectFeatures;
                            worksheet.Cells[row, 5].Value = material.ModelEngineeringQuantity;
                            worksheet.Cells[row, 6].Value = material.ModelEngineeringUnit;
                            row++;
                        }
                    }
                    // 假设已存在数据，获取已使用的单元格范围
                    ExcelRange usedRange = worksheet.Cells[worksheet.Dimension.Address];
                    int startRow = Math.Max(3, usedRange.Start.Row); // 确保起始行有效
                    int endRow = usedRange.End.Row;
                    int startCol = usedRange.Start.Column;
                    int endCol = usedRange.End.Column;
                    ExcelRange targetRange = worksheet.Cells[startRow, startCol, endRow, endCol];
                    // 检查是否有数据
                    if (targetRange != null)
                    {
                        // 为有效数据区域添加所有框线
                        targetRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        targetRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        targetRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        targetRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        targetRange.Style.Border.Diagonal.Style = OfficeOpenXml.Style.ExcelBorderStyle.None;
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
                    foreach (var (FamilyCategory, FamilyName, ExtendName, Parameters) in list)
                    {
                        worksheet.Cells[row, 1].Value = FamilyCategory;
                        worksheet.Cells[row, 2].Value = FamilyName;
                        worksheet.Cells[row, 3].Value = ExtendName;
                        if (!IsFamilyNameFalse)
                            worksheet.Cells[row, 4].Value = Parameters;
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
        public static ObservableCollection<EmbeddedCarbonCalcutionModel> ReadEmbeddedCarbonCalcutionExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var materials = new ObservableCollection<EmbeddedCarbonCalcutionModel>();
            if (!File.Exists(filePath))
            {
                TaskDialog.Show("错误", "文件不存在！");
                return null;
            }
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // 假设数据在第一个工作表中

                // 获取最大行数和列数
                var rowCount = worksheet.Dimension.Rows;
                var colCount = worksheet.Dimension.Columns;

                // 从第二行开始读取数据（第一行通常是表头）
                for (int row = 2; row <= rowCount; row++)
                {
                    var material = new EmbeddedCarbonCalcutionModel
                    {
                        // 按照属性顺序从 Excel 单元格中读取数据并赋值给对象属性
                        SerialNumber = worksheet.Cells[row, 1].GetValue<int>(),
                        MaterialName = worksheet.Cells[row, 2].GetValue<string>(),
                        SpecificationModel = worksheet.Cells[row, 3].GetValue<string>(),
                        MaterialCategory = worksheet.Cells[row, 4].GetValue<string>(),
                        SecondLevelCategory = worksheet.Cells[row, 5].GetValue<string>(),
                        Quantity = worksheet.Cells[row, 6].GetValue<decimal>(),
                        Unit = worksheet.Cells[row, 7].GetValue<string>(),
                        WeightConversionCoefficient = worksheet.Cells[row, 8].GetValue<decimal>(),
                        MaterialUnitPrice = worksheet.Cells[row, 9].GetValue<decimal>(),
                        CarbonEmission = worksheet.Cells[row, 10].GetValue<decimal>(),
                        MaterialTotalPrice = worksheet.Cells[row, 11].GetValue<decimal>(),
                        Weight = worksheet.Cells[row, 12].GetValue<decimal>(),
                        TransportationDistance = worksheet.Cells[row, 13].GetValue<decimal>(),
                        TransportationCarbonEmission = worksheet.Cells[row, 14].GetValue<decimal>()
                    };

                    materials.Add(material);
                }
            }
            return materials;
        }
        internal static string GetParentId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return string.Empty;

            // 按连字符分割ID
            string[] parts = id.Split('-');

            // 如果ID只有1部分，没有父级，返回空或原始ID（根据需求决定）
            if (parts.Length <= 1)
                return string.Empty;

            // 移除最后一部分
            string[] parentParts = new string[parts.Length - 1];
            Array.Copy(parts, parentParts, parts.Length - 1);

            // 重新组合父级ID
            return string.Join("-", parentParts);
        }
    }
}
