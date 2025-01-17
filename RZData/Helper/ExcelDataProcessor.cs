using Autodesk.Revit.UI;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace RZData.Models
{
    public static class ExcelDataProcessor
    {
        public static List<ExcelRecord> LoadDataFromExcel(ref string templatePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<ExcelRecord> records = new List<ExcelRecord>();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "选择Excel文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                templatePath = openFileDialog.FileName;

                using (var package = new ExcelPackage(new FileInfo(templatePath)))
                {
                    // 读取所有工作表
                    var worksheets = new[] { "族匹配表-装修", "族匹配表-机电", "族匹配表-结构" };
                    foreach (var sheetName in worksheets)
                    {
                        var worksheet = package.Workbook.Worksheets[sheetName];
                        if (worksheet != null)
                        {
                            ReadWorksheet(worksheet, records);
                        }
                    }
                }
            }

            return records;
        }

        private static void ReadWorksheet(ExcelWorksheet worksheet, List<ExcelRecord> records)
        {
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) // 假设第一行是标题行，从第二行开始读取
            {
                ExcelRecord excelRecord = new ExcelRecord
                {
                    FamilyName = worksheet.Cells[row, 2].Text, // B列
                    TypeName = worksheet.Cells[row, 3].Text, // C列
                    ExtendName = worksheet.Cells[row, 4].Text // D列
                };

                var requiredPropertie = worksheet.Cells[row, 7].Text; // G列
                excelRecord.RequiredProperties.Add(requiredPropertie);

                var nextFamilyName = row < rowCount ? worksheet.Cells[row + 1, 2].Text : null;

                while (string.IsNullOrWhiteSpace(nextFamilyName) && row < rowCount)
                {
                    row++;
                    requiredPropertie = worksheet.Cells[row, 7].Text; // G列
                    excelRecord.RequiredProperties.Add(requiredPropertie);
                    nextFamilyName = row < rowCount ? worksheet.Cells[row + 1, 2].Text : null;
                }

                records.Add(excelRecord);
            }
        }

        public static void ExportToExcel(DataElementData dataElementData)
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
                foreach (var family in dataElementData.Families)
                {
                    foreach (var familyType in family.FamilyTypes)
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
