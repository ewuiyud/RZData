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
        /// 关联的产品物料库
        /// </summary>
        public ExcelProductMaterialLibraryModel ProductMaterialLibrary { get; set; }
        /// <summary>
        /// 是否关联物料库
        /// </summary>
        public bool HasProductMaterialLibrary { get; set; }
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
        /// 损耗值
        /// </summary>
        public double LossValue { get; set; }

        /// <summary>
        /// 材料工程量
        /// </summary>
        public double MaterialQuantity { get; set; }

        /// <summary>
        /// 材料单位
        /// </summary>
        public string MaterialUnit { get; set; }

        /// <summary>
        /// 材料采购量
        /// </summary>
        public double MaterialProcurementQuantity { get; set; }

        /// <summary>
        /// 采购单位
        /// </summary>
        public string ProcurementUnit { get; set; }

        public ObservableCollection<RevitSolidElement> RevitSolidElements { get; set; }
        /// <summary>
        /// 户型
        /// </summary>
        public string Room { get; internal set; }
    }
}
