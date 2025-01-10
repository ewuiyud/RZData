using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RZData.Models
{
    public static class ExcelDataProcessor
    {
        public static List<ExcelRecord> LoadDataFromExcel()
        {
            List<ExcelRecord> records = new List<ExcelRecord>();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "选择Excel文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
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
    }
}
