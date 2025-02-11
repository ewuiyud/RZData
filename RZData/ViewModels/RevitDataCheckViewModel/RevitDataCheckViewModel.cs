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
using System;
using RZData.Services;

namespace RZData.ViewModels
{
    public class RevitDataCheckViewModel : BaseViewModel
    {
        private string _searchKeyword;

        private ElementViewModel _showElements;
        private object _selectedItem;

        public RevitDataCheckViewModel(UIDocument uiDocument, ObservableCollection<RevitSolidElement> AllSolidElements)
        {
            UiDocument = uiDocument;
            AllElements = new ElementViewModel(AllSolidElements.ToList());
            FamilyNameCheckElements = new ElementViewModel(AllSolidElements.ToList().FindAll(a => a.IsNameCorrect == false).ToList());
            ParametersCheckElements = new ElementViewModel(AllSolidElements.ToList().FindAll(a => a.IsNameCorrect == true && a.IsPropertiesCorrect == false).ToList());
            ShowParametersCheckElements = ParametersCheckElements;
            //commands
            SearchCommand = new RelayCommand(Search);
            ParameterExportCommand = new RelayCommand(ParameterExport);
            FamilyExportCommand = new RelayCommand(FamilyExport);
            PickObjectsCommand = new RelayCommand(PickObjects);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                SetProperty(ref _searchKeyword, value);
                if (_searchKeyword != "请输入关键词搜索")
                {
                    SearchCommand.Execute(null);
                }
            }
        }
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public ElementViewModel ShowParametersCheckElements { get => _showElements; set => SetProperty(ref _showElements, value); }
        public ICommand SearchCommand { get; }
        public ICommand ParameterExportCommand { get; }
        public ICommand FamilyExportCommand { get; }
        public ICommand PickObjectsCommand { get; }

        internal void PickObjects()
        {
            switch (SelectedItem)
            {
                case FamilyCategoryViewModel familyCategory:
                    break;
                case FamilyViewModel family:
                    SelectElementInRevit(family);
                    break;
                case FamilyExtendViewModel familyExtend:
                    SelectElementInRevit(familyExtend);
                    break;
                case ElementInstanceViewModel elementInstance:
                    SelectElementInRevit(elementInstance);
                    break;
                default:
                    break;
            }
        }
        private void SelectElementInRevit(FamilyViewModel family)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var id in family.IDs)
            {

                elementIds.Add(new ElementId(id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(FamilyExtendViewModel familyExtend)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var id in familyExtend.IDs)
            {
                elementIds.Add(new ElementId(id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(ElementInstanceViewModel elementInstance)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            elementIds.Add(new ElementId(elementInstance.Name));
            uidoc.Selection.SetElementIds(elementIds);
        }
        public void Search()
        {
            try
            {
                if (string.IsNullOrEmpty(SearchKeyword))
                {
                    ShowParametersCheckElements = ParametersCheckElements;
                }
                var revitSolidElements = ParametersCheckElements.RevitSolidElements.FindAll(a =>
                a.FamilyName.Contains(SearchKeyword) || a.FamilyCategory.Contains(SearchKeyword) || a.ExtendName.Contains(SearchKeyword));
                ShowParametersCheckElements = new ElementViewModel(revitSolidElements);
                Console.WriteLine(1);
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
                ExcelDataService.ExportToExcelFromElement(ShowParametersCheckElements, false);
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
                ExcelDataService.ExportToExcelFromElement(FamilyNameCheckElements);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
    }
}
