using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    public class ExcelMaterialBusinessRecord
    {
        /// <summary>
        /// 分类编码
        /// </summary>
        public string Code { get; set; }
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
        /// 工程量
        /// </summary>
        public string Quantity { get; set; }
    }
}
