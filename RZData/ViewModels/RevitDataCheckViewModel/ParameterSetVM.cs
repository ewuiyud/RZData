﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace RZData.ViewModels
{
    public class ParameterSetVM : ObservableObject
    {
        private string _value;
        private string _status;
        public string Name { get; set; }
        private ObservableCollection<string> Values { get; set; }
        public ObservableCollection<ParameterVM> Parameters { get; set; }
        public string ValueType { get; set; }
        public ParameterSetVM()
        {
            Values = new ObservableCollection<string>();
            Parameters = new ObservableCollection<ParameterVM>();
            Parameters.CollectionChanged += Parameters_CollectionChanged;
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
            Parameters.CollectionChanged += Parameters_CollectionChanged;
            UpdateValues();
        }

        private void Parameters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ParameterVM newItem in e.NewItems)
                {
                    newItem.PropertyChanged += Parameter_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ParameterVM oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= Parameter_PropertyChanged;
                }
            }
            UpdateValues();
        }
        private void Parameter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Parameter.Value))
            {
                UpdateValues();
            }
        }

        private void UpdateValues()
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
        }
        public string Status { get => _status; set => SetProperty(ref _status, value); }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
        public string ShowValue
        {
            get
            {
                if (string.IsNullOrEmpty(ValueType))
                {
                    return "缺失";
                }
                else
                {
                    return "正常";
                }
            }
        }

        public ParameterVM Parameter { get; }
    }
}
