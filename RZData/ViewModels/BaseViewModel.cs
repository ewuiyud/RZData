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
        private DataElement _allElements;
        private DataElement _familyNameCheckElements;
        private DataElement _parametersCheckElements;
        internal UIDocument UiDocument;

        public DataElement AllElements
        {
            get => _allElements;
            set => SetProperty(ref _allElements, value);
        }

        public DataElement FamilyNameCheckElements
        {
            get => _familyNameCheckElements;
            set => SetProperty(ref _familyNameCheckElements, value);
        }

        public DataElement ParametersCheckElements
        {
            get => _parametersCheckElements;
            set => SetProperty(ref _parametersCheckElements, value);
        }
    }
}
