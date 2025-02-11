using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    public class ParameterSet : ObservableObject
    {
        private string _value;
        public string Name { get; set; }
        public ObservableCollection<string> Values { get; set; }
        public string ValueType { get; set; }
        public ParameterSet()
        {
            Values = new ObservableCollection<string>();
            Values.CollectionChanged += Values_CollectionChanged;
        }
        private void Values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(ShowValue));
        }
        public string Status
        {
            get
            {
                if (Values.Count == 1)
                {
                    return "";
                }
                else
                {
                    return "多参数";
                }
            }
        }
        public string Value
        {
            get
            {
                if (string.IsNullOrEmpty(_value))
                {
                    if (Values.Count == 1)
                    {
                        return Values[0];
                    }
                    else
                    {
                        return $"[{string.Join(", ", Values)}]";
                    }
                }
                return _value;
            }
            set { _value = value; }
        }
        public string ShowValue
        {
            get
            {
                if (Value == "缺失")
                {
                    return "缺失";
                }
                else
                {
                    return "正常";
                }
            }
        }
    }
}
