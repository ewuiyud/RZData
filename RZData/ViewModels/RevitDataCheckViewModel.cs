using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using RZData.Models;
using RZData.Helper;
using System;
using System.Windows;

namespace RZData.ViewModels
{
    public class RevitDataCheckViewModel : BaseViewModel
    {
        private string _searchKeyword;
        private RevitTemplateLoadViewModel _revitTemplateLoadViewModel;
        private RevitDataEntryViewModel _revitDataEntryViewModel;
        private readonly UIDocument _uiDocument;
        private DataElementData _showElements;
        private object _selectedItem;
        private UIDocument uiDocument;

        public RevitDataCheckViewModel(UIDocument uiDocument, RevitTemplateLoadViewModel revitTemplateLoadViewModel)
        {
            _uiDocument = uiDocument;
            AllElements = revitTemplateLoadViewModel.AllElements;
            FamilyNameCheckElements = revitTemplateLoadViewModel.FamilyNameCheckElements;
            ParametersCheckElements = revitTemplateLoadViewModel.ParametersCheckElements;
            ShowParametersCheckElements = ParametersCheckElements;
            //commands
            SearchCommand = new RelayCommand(Search);
            ParameterExportCommand = new RelayCommand(ParameterExport);
            FamilyExportCommand = new RelayCommand(FamilyExport);
            //other viewModels
            RevitTemplateLoadViewModel = new RevitTemplateLoadViewModel(uiDocument);
            RevitDataEntryViewModel = new RevitDataEntryViewModel(uiDocument, AllElements);
        }

        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public DataElementData ShowParametersCheckElements { get => _showElements; set => SetProperty(ref _showElements, value); }
        public RevitTemplateLoadViewModel RevitTemplateLoadViewModel { get => _revitTemplateLoadViewModel; set => SetProperty(ref _revitTemplateLoadViewModel, value); }
        public RevitDataEntryViewModel RevitDataEntryViewModel { get => _revitDataEntryViewModel; set => SetProperty(ref _revitDataEntryViewModel, value); }
        public ICommand SearchCommand { get; }
        public ICommand ParameterExportCommand { get; }
        public ICommand FamilyExportCommand { get; }

        internal void DoubleClickAndPickObjects(object selectedValue)
        {
            switch (selectedValue)
            {
                case Models.Family family:
                    SelectElementInRevit(family);
                    break;
                case Models.FamilyType familyType:
                    SelectElementInRevit(familyType);
                    break;
                case FamilyExtend familyExtend:
                    SelectElementInRevit(familyExtend);
                    break;
                default:
                    break;
            }
        }
        private void SelectElementInRevit(Models.Family family)
        {
            var uidoc = _uiDocument;
            var elementIds = new List<ElementId>();
            foreach (var familyType in family.FamilyTypes)
            {
                foreach (var familyExtend in familyType.FamilyExtends)
                {
                    foreach (var item in familyExtend.DataInstances)
                    {
                        elementIds.Add(item.Element.Id);
                    }
                }
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(Models.FamilyType familyType)
        {
            var uidoc = _uiDocument;
            var elementIds = new List<ElementId>();
            foreach (var familyExtend in familyType.FamilyExtends)
            {
                foreach (var item in familyExtend.DataInstances)
                {
                    elementIds.Add(item.Element.Id);
                }
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(FamilyExtend familyExtend)
        {
            var uidoc = _uiDocument;
            var elementIds = new List<ElementId>();
            foreach (var item in familyExtend.DataInstances)
            {
                elementIds.Add(item.Element.Id);
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void Search()
        {
            try
            {
                ShowParametersCheckElements = ParametersCheckElements.Search(SearchKeyword);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        private void ParameterExport()
        {
            try
            {
                ExcelDataProcessor.ExportToExcel(ShowParametersCheckElements);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        private void FamilyExport()
        {
            try
            {
                ExcelDataProcessor.ExportToExcel(FamilyNameCheckElements);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
    }
}
