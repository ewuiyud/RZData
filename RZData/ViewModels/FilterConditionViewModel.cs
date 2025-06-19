using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class FilterConditionViewModel : ObservableObject
    {
        private string _propertyName;
        private string _propertyValue;
        private string _logic;
        public string PropertyName { get => _propertyName; set => SetProperty(ref _propertyName, value); }
        public string PropertyValue { get => _propertyValue; set => SetProperty(ref _propertyValue, value); }
        public string Logic { get => _logic; set => SetProperty(ref _logic, value); }
    }
}
