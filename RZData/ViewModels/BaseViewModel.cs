using Autodesk.Revit.UI;
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
        private ElementViewModel _allElements;
        private ElementViewModel _familyNameCheckElements;
        private ElementViewModel _parametersCheckElements;
        internal UIDocument UiDocument;

        public ElementViewModel AllElements
        {
            get => _allElements;
            set => SetProperty(ref _allElements, value);
        }

        public ElementViewModel FamilyNameCheckElements
        {
            get => _familyNameCheckElements;
            set => SetProperty(ref _familyNameCheckElements, value);
        }

        public ElementViewModel ParametersCheckElements
        {
            get => _parametersCheckElements;
            set => SetProperty(ref _parametersCheckElements, value);
        }
    }
}
