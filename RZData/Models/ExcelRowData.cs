using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    /// <summary>
    /// Excel数据行模型
    /// </summary>
    public class ExcelRowData
    {
        public int RowIndex { get; set; }
        public string CategoryName { get; set; }
        public string FamilyName { get; set; }
        public string ParameterName { get; set; }
        public bool IsMergedCell { get; set; }
    }
}
