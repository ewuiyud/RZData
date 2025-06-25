using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    public class ExcelMaterialBusinessModel
    {
        /// <summary>
        /// 分类编码
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// TDC-元素分类名称
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        /// TDC-产品分类名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// TDC-空间分类名称
        /// </summary>
        public string SpaceName { get; set; }

        /// <summary>
        /// 补充链接规则
        /// </summary>
        public string ExtendRule { get; set; }

        /// <summary>
        /// TDC-项目特征
        /// </summary>
        public string ProjectCharacteristics { get; set; }

        /// <summary>
        /// TDC-使用位置
        /// </summary>
        public string UsageLocation { get; set; }

        /// <summary>
        /// 模型工程量
        /// </summary>
        public string ModelEngineeringQuantity { get; set; }

        /// <summary>
        /// 模型工程量单位
        /// </summary>
        public string ModelEngineeringUnit { get; set; }

        /// <summary>
        /// 损耗值
        /// </summary>
        public string LossValue { get; set; }

        /// <summary>
        /// 材料工程量（有库）
        /// </summary>
        public string MaterialQuantityHasLibrary { get; set; }

        /// <summary>
        /// 材料工程量（无库）
        /// </summary>
        public string MaterialQuantityNoLibrary { get; set; }

        /// <summary>
        /// 材料单位
        /// </summary>
        public string MaterialUnit { get; set; }

        /// <summary>
        /// 材料采购量（有库）
        /// </summary>
        public string MaterialProcurementQuantityHasLibrary { get; set; }

        /// <summary>
        /// 材料采购量（无库）
        /// </summary>
        public string MaterialProcurementQuantityNoLibrary { get; set; }

        /// <summary>
        /// 采购单位
        /// </summary>
        public string ProcurementUnit { get; set; }
    }
}
