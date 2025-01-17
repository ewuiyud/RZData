using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using OfficeOpenXml;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class ViewModelLocator : ObservableObject
    {
        private static ViewModelLocator _instance;
        private UIDocument _uiDocument;
        public static ViewModelLocator Instance(UIDocument uiDocument)
        {
            if (_instance == null)
            {
                _instance = new ViewModelLocator(uiDocument);
            }
            return _instance;
        }
        public ViewModelLocator(UIDocument _uiDocument)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            this._uiDocument = _uiDocument;
            RevitTemplateLoadViewModel = new RevitTemplateLoadViewModel(_uiDocument);
            RevitDataCheckViewModel = new RevitDataCheckViewModel(_uiDocument, RevitTemplateLoadViewModel);
            RevitDataEntryViewModel = new RevitDataEntryViewModel(_uiDocument, RevitTemplateLoadViewModel.AllElements);
        }
        public void Reset(RevitTemplateLoadViewModel revitTemplateLoadViewModel)
        {
            RevitDataCheckViewModel = new RevitDataCheckViewModel(_uiDocument, RevitTemplateLoadViewModel);
            RevitDataEntryViewModel = new RevitDataEntryViewModel(_uiDocument, RevitTemplateLoadViewModel.AllElements);
        }

        public RevitTemplateLoadViewModel RevitTemplateLoadViewModel { get; }
        public RevitDataEntryViewModel RevitDataEntryViewModel { get; set; }
        public RevitDataCheckViewModel RevitDataCheckViewModel { get; set; }
    }
}
