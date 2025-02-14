﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    public class Parameter : ObservableObject
    {
        private string _value;
        public string Value { get => _value; set => SetProperty(ref _value, value); }
        public string Name { get; set; }
        public string TDCName { get; set; }
        public string ValueType { get; set; }
    }
}
