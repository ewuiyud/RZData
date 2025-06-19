using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    public class ExcelProductMaterialLibraryModel
    {
        /// <summary>
        /// 物料名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 物料品牌
        /// </summary>
        public string Brand { get; set; }
        /// <summary>
        /// 规格属性
        /// </summary>
        public string SpecificationAttributes { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNumber { get; set; }
    }
}
