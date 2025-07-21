using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class ParameterVM : ObservableObject
    {
        public ParameterVM()
        {
            ValueEnum = new List<string>();
        }

        private string _value;
        public string Value
        {
            get => _value; set
            {
                SetProperty(ref _value, value);
            }
        }
        public string Name { get; set; }
        public string TDCName { get; set; }
        public string ValueType { get; set; }
        /// <summary>
        /// 单位   
        /// /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 是否在项目特征中显示
        /// </summary>
        public bool IsShowed { get; set; } = true;
        /// <summary>
        /// 有预设值的都是只读的
        /// </summary>
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// 参考值
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// 枚举值列表
        /// </summary>
        public List<string> ValueEnum { get; set; }
    }
}
