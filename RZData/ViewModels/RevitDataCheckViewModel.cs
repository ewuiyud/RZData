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
    public class RevitDataCheckViewModel : ObservableObject
    {
        // 静态全局实例
        private static RevitDataCheckViewModel _instance;
        public static RevitDataCheckViewModel Instance()
        {
            if (_instance == null)
            {
                MessageBox.Show("请先初始化RevitDataCheckViewModel");
            }
            return _instance;
        }
        public static RevitDataCheckViewModel Instance(UIDocument uiDocument)
        {
            if (_instance == null)
            {
                _instance = new RevitDataCheckViewModel(uiDocument);
            }
            return _instance;
        }

        private string _searchKeyword;
        private RevitTemplateLoadViewModel _revitTemplateLoadViewModel;
        private RevitDataEntryViewModel _revitDataEntryViewModel;
        private readonly UIDocument _uiDocument;
        private DataElementData _showElements;
        private DataElementData _familyNameCheckElements;
        private DataElementData _parametersCheckElements;
        private DataElementData _allElements;
        private object _selectedItem;
        public RevitDataCheckViewModel(UIDocument uiDocument)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _uiDocument = uiDocument;
            ShowElements = new DataElementData();
            AllElements = new DataElementData();
            FamilyNameCheckElements = new DataElementData();
            ParametersCheckElements = new DataElementData();
            //commands
            SearchCommand = new RelayCommand(Search);
            ParameterExportCommand = new RelayCommand(ParameterExport);
            FamilyExportCommand = new RelayCommand(FamilyExport);
            //other viewModels
            RevitTemplateLoadViewModel = new RevitTemplateLoadViewModel(uiDocument);
            RevitDataEntryViewModel = new RevitDataEntryViewModel(uiDocument, AllElements);
        }

        public DataElementData AllElements { get => _allElements; set => SetProperty(ref _allElements, value); }
        public DataElementData FamilyNameCheckElements { get => _familyNameCheckElements; set => SetProperty(ref _familyNameCheckElements, value); }
        public DataElementData ParametersCheckElements { get => _parametersCheckElements; set => SetProperty(ref _parametersCheckElements, value); }
        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public DataElementData ShowElements { get => _showElements; set => SetProperty(ref _showElements, value); }
        public RevitTemplateLoadViewModel RevitTemplateLoadViewModel { get => _revitTemplateLoadViewModel; set => SetProperty(ref _revitTemplateLoadViewModel, value); }
        public RevitDataEntryViewModel RevitDataEntryViewModel { get => _revitDataEntryViewModel; set => SetProperty(ref _revitDataEntryViewModel, value); }
        public ICommand SearchCommand { get; }
        public ICommand ParameterExportCommand { get; }
        public ICommand FamilyExportCommand { get; }

        //根据族名来检验模型
        public void CheckModel(List<ExcelRecord> records)
        {
            var systemFamilyDictionary = records.FindAll(a => !a.TypeName.StartsWith("MIC"));
            var loadableFamilyDictionary = records.FindAll(a => a.TypeName.StartsWith("MIC"));
            var familyList = new List<string>();
            records.ForEach(a => { if (!familyList.Contains(a.FamilyName)) familyList.Add(a.FamilyName); });
            var document = _uiDocument.Document;
            var collector = new FilteredElementCollector(document);
            var elements = collector.WhereElementIsNotElementType();

            AllElements.Clear();
            ShowElements.Clear();
            FamilyNameCheckElements.Clear();
            ParametersCheckElements.Clear();
            DataElementData revitElementData = new DataElementData();

            foreach (var element in elements)
            {
                if (familyList.Contains(element.GetFamily()))
                {
                    var dataInstance = AllElements.Add(element);
                    if (element is FamilyInstance familyInstance)
                    {
                        var typeName = element.GetFamilyType();
                        var record = loadableFamilyDictionary.FirstOrDefault(a => typeName.StartsWith(a.TypeName.Substring(0, a.TypeName.Length - 1)));
                        if (record == null || element.GetFamily() != record.FamilyName)
                        {
                            dataInstance.FamilyExtend.IsNameCorrect = false;
                            FamilyNameCheckElements.Add(dataInstance);
                        }
                        else
                        {
                            dataInstance.FamilyExtend.IsNameCorrect = true;
                            if (!dataInstance.CheckParameters(record,document))
                            {
                                ParametersCheckElements.Add(dataInstance);
                            }
                        }
                    }
                    else
                    {
                        var extendName = element.GetExtendName();
                        var typeNames = systemFamilyDictionary.FindAll(a => CheckRecordExtendName(a, element)).ToList();
                        if (typeNames.Count() == 0 || !typeNames.Exists(a => a.TypeName == element.GetFamilyType()))
                        {
                            dataInstance.FamilyExtend.IsNameCorrect = false;
                            FamilyNameCheckElements.Add(dataInstance);
                        }
                        else
                        {
                            var record = typeNames.First(a => a.TypeName == element.GetFamilyType());
                            dataInstance.FamilyExtend.IsNameCorrect = true;
                            if (!dataInstance.CheckParameters(record, document))
                            {
                                ParametersCheckElements.Add(dataInstance);
                            }
                        }
                    }
                }
            }
            AllElements.MergeParameters();
            FamilyNameCheckElements.MergeParameters();
            ParametersCheckElements.MergeParameters();
            ShowElements = ParametersCheckElements;
        }
        private bool CheckRecordExtendName(ExcelRecord excelRecord, Element element)
        {
            string incorrectMessage = $"补充属性不合理，族：{excelRecord.FamilyName} 类型：{excelRecord.TypeName} 补充属性：{excelRecord.ExtendName}";
            const string typePrefix = "类型=";
            var recordExtendName = excelRecord.ExtendName;
            if (recordExtendName.Contains("&&"))
            {
                var requires = recordExtendName.Split(new[] { "&&" }, StringSplitOptions.None);
                return requires.Any(a =>
                {
                    if (a.StartsWith(typePrefix))
                    {
                        return element.GetExtendName().StartsWith(a.Substring(3, a.Length - 4));
                    }
                    else
                    {
                        var str = a.Split('=');
                        if (str.Count() != 2)
                            MessageBox.Show(incorrectMessage);
                        var value = element.GetParameters(str[0]);
                        if (value.Count == 0)
                        {
                            return false;
                        }
                        return value[0]?.Element?.Name == str[1];
                    }
                });
            }
            else
            {
                if (recordExtendName.StartsWith(typePrefix))
                {
                    var d = recordExtendName.Substring(3, recordExtendName.Length - 4);
                    var c = element.GetExtendName();
                    return element.GetExtendName().StartsWith(recordExtendName.Substring(3, recordExtendName.Length - 4));
                }
                else { MessageBox.Show(incorrectMessage); return false; }
            }
        }

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
            ShowElements = ParametersCheckElements.Search(SearchKeyword);
        }
        private void ParameterExport()
        {
            ExcelDataProcessor.ExportToExcel(ShowElements);
        }
        private void FamilyExport()
        {
            ExcelDataProcessor.ExportToExcel(FamilyNameCheckElements);
        }
    }
}
