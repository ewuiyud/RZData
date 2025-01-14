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
        public static RevitDataCheckViewModel Instance(UIDocument uiDocument)
        {
            if (_instance == null)
            {
                _instance = new RevitDataCheckViewModel(uiDocument);
            }
            return _instance;
        }
        public RevitTemplateLoadViewModel revitTemplateLoadViewModel;
        private readonly UIDocument _uiDocument;
        private DataElementData _elements;
        public ObservableCollection<string> ComboboxOptions { get; set; }
        private string _comboboxOption;
        private object _selectedItem;
        private string _templatePath;
        public RevitDataCheckViewModel(UIDocument uiDocument)
        {
            RevitTemplateLoadViewModel = new RevitTemplateLoadViewModel();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _uiDocument = uiDocument;
            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            ShowElements = new DataElementData();
            AllElements = new DataElementData();
            FamilyNameCheckElements = new DataElementData();
            ParametersCheckElements = new DataElementData();
            ComboboxOptions = new ObservableCollection<string>
            {
                ComboboxOptionEnum.族匹配检验.ToString(),
                ComboboxOptionEnum.属性项校验.ToString(),
            };
            ComboboxOption = ComboboxOptionEnum.族匹配检验.ToString();
        }

        private DataElementData AllElements { get; set; }
        private DataElementData FamilyNameCheckElements { get; set; }
        private DataElementData ParametersCheckElements { get; set; }
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public string TemplatePath { get => _templatePath; set => SetProperty(ref _templatePath, value); }
        public DataElementData ShowElements { get => _elements; set => SetProperty(ref _elements, value); }
        public string ComboboxOption { get => _comboboxOption; set => SetProperty(ref _comboboxOption, value); }
        public RevitTemplateLoadViewModel RevitTemplateLoadViewModel { get => revitTemplateLoadViewModel; set => SetProperty(ref revitTemplateLoadViewModel, value); }
        public ICommand LoadDataFromExcelCommand { get; }

        /// <summary>
        /// 从Excel文件加载数据,并检测模型
        /// </summary>
        private void LoadDataFromExcel()
        {
            string path = "";
            List<ExcelRecord> records = ExcelDataProcessor.LoadDataFromExcel(ref path);
            TemplatePath = path;
            CheckModel(records);
        }

        //根据族名来检验模型
        private void CheckModel(List<ExcelRecord> records)
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
                            if (!dataInstance.CheckParameters(record))
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
                            if (!dataInstance.CheckParameters(record))
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
            ComboboxOption = ComboboxOptionEnum.属性项校验.ToString();
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

        internal void OptionChanged()
        {
            switch (ComboboxOption)
            {
                case "族匹配检验":
                    ShowElements = FamilyNameCheckElements;
                    break;
                case "属性项校验":
                    ShowElements = ParametersCheckElements;
                    break;
                default:
                    break;
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
    }
    public enum ComboboxOptionEnum
    {
        族匹配检验,
        属性项校验,
    }
}
