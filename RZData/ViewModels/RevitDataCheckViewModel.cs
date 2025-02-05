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

        private DataElement _showElements;
        private object _selectedItem;

        public RevitDataCheckViewModel(UIDocument uiDocument, BaseViewModel baseViewModel)
        {
            UiDocument = uiDocument;
            AllElements = baseViewModel.AllElements;
            FamilyNameCheckElements = baseViewModel.FamilyNameCheckElements;
            ParametersCheckElements = baseViewModel.ParametersCheckElements;
            ShowParametersCheckElements = ParametersCheckElements;
            //commands
            SearchCommand = new RelayCommand(Search);
            ParameterExportCommand = new RelayCommand(ParameterExport);
            FamilyExportCommand = new RelayCommand(FamilyExport);
        }

        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public DataElement ShowParametersCheckElements { get => _showElements; set => SetProperty(ref _showElements, value); }
        public RevitTemplateLoadViewModel RevitTemplateLoadViewModel { get => _revitTemplateLoadViewModel; set => SetProperty(ref _revitTemplateLoadViewModel, value); }
        public RevitDataEntryViewModel RevitDataEntryViewModel { get => _revitDataEntryViewModel; set => SetProperty(ref _revitDataEntryViewModel, value); }
        public ICommand SearchCommand { get; }
        public ICommand ParameterExportCommand { get; }
        public ICommand FamilyExportCommand { get; }

        internal void DoubleClickAndPickObjects(object selectedValue)
        {
            switch (selectedValue)
            {
                case Models.FamilyCategory family:
                    SelectElementInRevit(family);
                    break;
                case Models.Family familyType:
                    SelectElementInRevit(familyType);
                    break;
                case FamilyExtend familyExtend:
                    SelectElementInRevit(familyExtend);
                    break;
                default:
                    break;
            }
        }
        private void SelectElementInRevit(Models.FamilyCategory family)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var familyType in family.Families)
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
        private void SelectElementInRevit(Models.Family familyType)
        {
            var uidoc = UiDocument;
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
            var uidoc = UiDocument;
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
                ExcelDataHelper.ExportToExcel(ShowParametersCheckElements);
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
                ExcelDataHelper.ExportToExcel(FamilyNameCheckElements);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
    }
}
