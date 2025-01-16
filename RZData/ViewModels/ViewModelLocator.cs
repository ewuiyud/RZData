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
    public class ViewModelLocator:ObservableObject
    {
        private static ViewModelLocator _instance;

        public static ViewModelLocator Instance()
        {
            if (_instance == null)
            {
                TaskDialog.Show("提醒","请先初始化RevitDataCheckViewModel");
            }
            return _instance;
        }
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
            RevitTemplateLoadViewModel = new RevitTemplateLoadViewModel(_uiDocument);
            RevitDataCheckViewModel = new RevitDataCheckViewModel(_uiDocument);
            RevitDataEntryViewModel = new RevitDataEntryViewModel( _uiDocument,  AllElements);
        }

        public RevitTemplateLoadViewModel RevitTemplateLoadViewModel { get; }
        public RevitDataEntryViewModel RevitDataEntryViewModel { get; }
        public RevitDataCheckViewModel RevitDataCheckViewModel { get; }
    }
}
