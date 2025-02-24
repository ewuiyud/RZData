using Autodesk.Revit.UI;
using OfficeOpenXml;
using RZData.Models;
using System.Collections.ObjectModel;
using RZData.Services;

namespace RZData.ViewModels
{
    public class ViewModelLocator : BaseViewModel
    {
        private static ViewModelLocator _instance;
        public static ViewModelLocator Instance(UIDocument uiDocument)
        {
            if (_instance == null)
            {
                _instance = new ViewModelLocator(uiDocument);
            }
            _instance.UiDocument = uiDocument;
            return _instance;
        }
        public ObservableCollection<RevitSolidElement> AllSolidElements { get; set; }
        /// <summary>
        /// 族名称错误的元素
        /// </summary>
        public ObservableCollection<RevitSolidElement> FamilyNameErrorElements { get; set; }
        /// <summary>
        /// 参数错误的元素
        /// </summary>
        public ObservableCollection<RevitSolidElement> ParametersErrorElements { get; set; }
        public ViewModelLocator(UIDocument _uiDocument)
        {
            this.UiDocument = _uiDocument;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            AllSolidElements = new ObservableCollection<RevitSolidElement>();
            RevitTemplateLoadViewModel = new RevitTemplateLoadViewModel();
            this.Reset();
        }

        public RevitTemplateLoadViewModel RevitTemplateLoadViewModel { get; }
        public RevitDataEntryViewModel RevitDataEntryViewModel { get; set; }
        public RevitDataCheckViewModel RevitDataCheckViewModel { get; set; }
        public RevitListSummaryViewModel RevitListSummaryViewModel { get; set; }

        public void Reset()
        {
            RevitElementService _revitElementService = new RevitElementService();
            AllSolidElements = _revitElementService.LoadAllRevitElements(UiDocument);
            RevitDataCheckViewModel = new RevitDataCheckViewModel(UiDocument, AllSolidElements);
            RevitDataEntryViewModel = new RevitDataEntryViewModel(UiDocument, AllSolidElements);
            RevitListSummaryViewModel = new RevitListSummaryViewModel(UiDocument, AllSolidElements);
        }
    }
}
