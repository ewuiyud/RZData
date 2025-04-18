﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    public class MaterialRecord
    {
        public MaterialRecord()
        {
            DataInstances = new ObservableCollection<DataInstance>();
            ProjectFeaturesDetail = new Dictionary<string, string>();
        }
        /// <summary>
        /// 材料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 使用方式
        /// </summary>
        public string UsageMethod { get; set; }

        /// <summary>
        /// 项目特征
        /// </summary>
        public string ProjectFeatures
        {
            get
            {
                string result = "";
                int index = 1;
                foreach (var item in ProjectFeaturesDetail)
                {
                    result += $"{index}、{item.Key}:{item.Value}\n";
                    index++;
                }
                return result;
            }
        }
        /// <summary>
        /// 项目特征具体数据
        /// </summary>
        public Dictionary<string, string> ProjectFeaturesDetail { get; set; }
        /// <summary>
        /// 模型工程量
        /// </summary>
        public double ModelEngineeringQuantity { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 转换规则
        /// </summary>
        public string ConversionRule { get; set; }

        /// <summary>
        /// 损耗值
        /// </summary>
        public double LossValue { get; set; }

        /// <summary>
        /// 材料量
        /// </summary>
        public double MaterialQuantity { get; set; }

        /// <summary>
        /// 材料单位
        /// </summary>
        public string MaterialUnit { get; set; }
        public ObservableCollection<DataInstance> DataInstances { get; set; }
    }

    public class AssemblyRecord
    {
        /// <summary>
        /// 构件名称
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 所属模型
        /// </summary>
        public string Modelbelonging { get; set; }

        /// <summary>
        /// 构件ID
        /// </summary>
        public string AssemblyID { get; set; }
    }
}
