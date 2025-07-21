using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace RZData.ViewModels
{
    public class ParameterSetVM : ObservableObject
    {
        private bool _isModified;
        public bool IsModified
        {
            get => _isModified;
            set => SetProperty(ref _isModified, value);
        }
        private string _value;
        public string Value
        {
            get => _value; set
            {
                IsModified = true;
                SetProperty(ref _value, value);
            }
        }
        public string Name { get; set; }
        public string TDCName { get; set; }
        public string ValueType { get; set; }
        private string _status;
        private ObservableCollection<string> Values { get; set; }
        public ObservableCollection<ParameterVM> Parameters { get; set; }
        public ParameterSetVM()
        {
            Values = new ObservableCollection<string>();
            Parameters = new ObservableCollection<ParameterVM>();
            IsModified = false;
        }

        public ParameterSetVM(ParameterVM parameter)
        {
            Value = parameter.Value;
            Values = new ObservableCollection<string>() { parameter.Value };
            Parameters = new ObservableCollection<ParameterVM>
            {
                parameter
            };
            Name = parameter.Name;
            ValueType = parameter.ValueType;
            UpdateValues();
            IsModified = false;
        }

        public void UpdateValues()
        {
            Values.Clear();
            foreach (var parameter in Parameters)
            {
                if (!Values.Contains(parameter.Value))
                {
                    Values.Add(parameter.Value);
                }
            }
            if (Values.Count > 1)
            {
                Status = "多参数";
                Value = $"[{string.Join(", ", Values)}]";
            }
            else
            {
                Status = "";
                Value = Values[0];
            }
            IsModified = false;
        }
        public string Status { get => _status; set => SetProperty(ref _status, value); }

        public string ShowValue
        {
            get
            {
                if (Value == ConstString.LossParameterName)
                {
                    return ConstString.LossParameterName;
                }
                else
                {
                    return ConstString.NormalParameterName ;
                }
            }
        }

        /// <summary>
        /// 单位   
        /// /// </summary>
        public string Unit { get; set; }
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
