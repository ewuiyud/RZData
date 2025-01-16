using CommunityToolkit.Mvvm.ComponentModel;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class BaseViewModel : ObservableObject
    {
        private DataElementData _allElements;
        private DataElementData _familyNameCheckElements;
        private DataElementData _parametersCheckElements;

        public DataElementData AllElements
        {
            get => _allElements;
            set => SetProperty(ref _allElements, value);
        }

        public DataElementData FamilyNameCheckElements
        {
            get => _familyNameCheckElements;
            set => SetProperty(ref _familyNameCheckElements, value);
        }

        public DataElementData ParametersCheckElements
        {
            get => _parametersCheckElements;
            set => SetProperty(ref _parametersCheckElements, value);
        }
    }
}
