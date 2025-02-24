using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class EmbeddedCarbonCalcutionModel
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int SerialNumber { get; set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string SpecificationModel { get; set; }

        /// <summary>
        /// 材料类别
        /// </summary>
        public string MaterialCategory { get; set; }

        /// <summary>
        /// 二级分类
        /// </summary>
        public string SecondLevelCategory { get; set; }

        private decimal quantity;
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity
        {
            get
            {
                return Math.Round(quantity, 2);
            }
            set => quantity = value;
        }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 重量换算系数(vunit/Q)
        /// </summary>
        public decimal WeightConversionCoefficient { get; set; }
        /// <summary>
        /// 材料单价(元)
        /// </summary>
        public decimal MaterialUnitPrice { get; set; }

        /// <summary>
        /// 碳排放量
        /// </summary>
        public decimal CarbonEmission { get; set; }
        /// <summary>
        /// 材料合价(元)
        /// </summary>
        public decimal MaterialTotalPrice { get; set; }
        private decimal weight;
        /// <summary>
        /// 重量(t)
        /// </summary>
        public decimal Weight
        {
            get
            {
                return Math.Round(weight, 2);
            }
            set => weight = value;
        }
        /// <summary>
        /// 运输距离
        /// </summary>
        public decimal TransportationDistance { get; set; }

        private decimal transportationCarbonEmission;
        /// <summary>
        /// 运输碳排放量
        /// </summary>
        public decimal TransportationCarbonEmission
        {
            get
            {
                return Math.Round(transportationCarbonEmission,3);
            }
            set => transportationCarbonEmission = value;
        }
    }
}
