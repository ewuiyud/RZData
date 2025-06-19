using CommunityToolkit.Mvvm.ComponentModel;
using RZData.Models;
using RZData.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class MaterialViewModel : ObservableObject
    {
        public MaterialViewModel()
        {
            RevitSolidElements = new ObservableCollection<RevitSolidElement>();
            ProjectFeaturesDetail = new Dictionary<string, string>();
        }
        /// <summary>
        /// 分类编码
        /// </summary>
        public string ID;
        /// <summary>
        /// 父级的分类编码
        /// </summary>
        public string FatherID;
        /// <summary>
        /// 材料名称
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 是否匹配物料库
        /// </summary>
        public bool IsMatchMaterialLibrary { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 关联的物料库
        /// </summary>
        public string ProductLibrary { get; set; }
        /// <summary>
        /// 可关联物料库的名称列表
        /// </summary>
        public List<string> ProductLibraryList
        {
            get
            {
                List<string> list = new List<string>();
                if (ProductName != null)
                {
                    ExcelDataService.ExcelProductMaterialLibraryModels.FindAll(x => x.ProductName == ProductName)
                        .ForEach(x => list.Add(x.Name));
                }
                //允许选空
                if (list.Count != 0)
                {
                    list.Add("");
                }
                return list;
            }
        }

        /// <summary>
        /// 使用方式
        /// </summary>
        public string UsageMethod { get; set; }
        /// <summary>
        /// 项目特征
        /// </summary>
        public string ProjectFeatures { get; set; }
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
        public string ModelEngineeringUnit { get; set; }

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
        public ObservableCollection<RevitSolidElement> RevitSolidElements { get; set; }
    }
}
