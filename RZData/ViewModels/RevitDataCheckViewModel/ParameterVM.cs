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
        public bool _isModified;
        private ParameterVM()
        {

        }
        public ParameterVM(string name, string value, string tDCName, string valueType)
        {
            Value = value;
            Name = name;
            TDCName = tDCName;
            ValueType = valueType;
            IsModified = false;
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
        public bool IsModified { get => _isModified; set => SetProperty(ref _isModified, value); }
    }
}
