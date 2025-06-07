using OfficeOpenXml;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Services
{
    /// <summary>
    /// Excel数据处理服务
    /// </summary>
    public class ExcelDataProcessor
    {
        /// <summary>
        /// 读取Excel文件并解析数据
        /// </summary>
        /// <param name="filePath">Excel文件路径</param>
        /// <returns>族属性数据列表</returns>
        public List<InsetParameterData> ProcessExcelFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Excel文件不存在: {filePath}");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["族匹配表-装修"];
                var rawData = ExtractRawData(worksheet);
                return ProcessMergedCellData(rawData);
            }
        }

        /// <summary>
        /// 提取原始数据
        /// </summary>
        private List<ExcelRowData> ExtractRawData(ExcelWorksheet worksheet)
        {
            var rawData = new List<ExcelRowData>();
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) // 假设第一行是标题
            {
                var categoryName = worksheet.Cells[row, 2].Text?.Trim(); // B列
                var familyName = worksheet.Cells[row, 3].Text?.Trim();   // C列
                var propertyName = worksheet.Cells[row, 7].Text?.Trim(); // G列

                if (!string.IsNullOrEmpty(propertyName))
                {
                    rawData.Add(new ExcelRowData
                    {
                        RowIndex = row,
                        CategoryName = categoryName,
                        FamilyName = familyName,
                        ParameterName = propertyName
                    });
                }
            }

            return rawData;
        }

        /// <summary>
        /// 处理合并单元格数据
        /// </summary>
        private List<InsetParameterData> ProcessMergedCellData(List<ExcelRowData> rawData)
        {
            var result = new List<InsetParameterData>();
            string currentCategory = "";
            string currentFamily = "";
            string currentParameter = "";

            foreach (var row in rawData.OrderBy(r => r.RowIndex))
            {
                // 更新当前类别和族名称（如果不为空）
                if (!string.IsNullOrEmpty(row.CategoryName))
                    currentCategory = row.CategoryName;
                if (!string.IsNullOrEmpty(row.FamilyName))
                    currentFamily = row.FamilyName;
                if (!string.IsNullOrEmpty(row.ParameterName))
                    currentParameter = row.ParameterName;

                // 查找或创建对应的族属性数据
                var familyData = result.FirstOrDefault(f =>
                    f.ParameterName == currentParameter);

                if (familyData == null)
                {
                    familyData = new InsetParameterData
                    {
                        ParameterName = currentParameter,
                        CategoryNames = new List<string> { currentCategory },
                    };
                    result.Add(familyData);
                }

                // 添加属性项
                if (!familyData.CategoryNames.Contains(currentCategory))
                {
                    familyData.CategoryNames.Add(currentCategory);
                }
            }

            return result;
        }

        /// <summary>
        /// 验证数据完整性
        /// </summary>
        public bool ValidateData(List<InsetParameterData> data)
        {
            return data.All(f =>
                !string.IsNullOrEmpty(f.ParameterName) &&
                f.CategoryNames.Any());
        }
    }
}
