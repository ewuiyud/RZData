using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 测试
{
    internal class Program
    {
        static Program()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public static List<(string, string,string)> SystemFamilyDictionary = new List<(string, string, string)>();
        public static List<(string, string, string)> LoadableFamilyDictionary = new List<(string, string, string)>();
        [STAThread]
        static void Main(string[] args)
        {
            LoadDataFromExcel();
        }
        private static void LoadDataFromExcel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "选择Excel文件"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    // 读取 "族匹配表-装修" 工作表
                    var worksheet1 = package.Workbook.Worksheets["族匹配表-装修"];
                    if (worksheet1 != null)
                    {
                        ReadWorksheet(worksheet1);
                    }

                    // 读取 "族匹配表-机电" 工作表
                    var worksheet2 = package.Workbook.Worksheets["族匹配表-机电"];
                    if (worksheet2 != null)
                    {
                        ReadWorksheet(worksheet2);
                    }

                    // 读取 "族匹配表-结构" 工作表
                    var worksheet3 = package.Workbook.Worksheets["族匹配表-结构"];
                    if (worksheet3 != null)
                    {
                        ReadWorksheet(worksheet3);
                    }
                }
            }
        }
        private static void ReadWorksheet(ExcelWorksheet worksheet)
        {
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            for (int row = 2; row <= rowCount; row++) // 假设第一行是标题行，从第二行开始读取
            {
                var familyName = worksheet.Cells[row, 2].Text; // B列
                var typeName = worksheet.Cells[row, 3].Text; // C列
                var extendName = worksheet.Cells[row, 4].Text; // D列
                if (!string.IsNullOrWhiteSpace(familyName) && !string.IsNullOrWhiteSpace(typeName)&& !string.IsNullOrWhiteSpace(extendName))
                {
                    if (extendName != "不填")
                    {
                        // 匹配系统族
                        SystemFamilyDictionary.Add((familyName,typeName, extendName));
                    }
                    else
                    {
                        // 匹配载入族
                        LoadableFamilyDictionary.Add((familyName, typeName, extendName));
                    }
                }
            }
        }
    }
}


