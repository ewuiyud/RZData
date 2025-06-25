using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    /// <summary>
    /// 有产品库的数据
    /// </summary>
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
        /// 项目特征具体数据
        /// </summary>
        public Dictionary<string, string> SpecificationAttributesDetail { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 材料工程量
        /// </summary>
        public double MaterialQuantity { get; set; }

        /// <summary>
        /// 材料采购量
        /// </summary>
        public double MaterialProcurementQuantity { get; set; }
    }
}
