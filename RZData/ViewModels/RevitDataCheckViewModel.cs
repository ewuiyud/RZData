using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Architecture;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using RZData.Models;
using RZData.Helper;
using System.Xml.Linq;
using System;
using System.Windows;
using System.CodeDom;

namespace RZData.ViewModels
{
    //todo:
    //1. 多个实体同一个族的情况，需要处理
    //2. 属性的匹配问题
    //3. 界面绘制和优化
    public class RevitDataCheckViewModel : ObservableObject
    {
        private readonly UIDocument _uiDocument;
        private DataElementData _elements;
        public ObservableCollection<string> ComboboxOptions { get; set; }
        private string _comboboxOption;
        private object _selectedItem;

        public RevitDataCheckViewModel(UIDocument uiDocument)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _uiDocument = uiDocument;
            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            Elements = new DataElementData();
            AllElements = new DataElementData();
            ComboboxOptions = new ObservableCollection<string>
            {
                ComboboxOptionEnum.所有.ToString(),
                ComboboxOptionEnum.族匹配缺失.ToString(),
                ComboboxOptionEnum.属性项匹配缺失.ToString(),
                ComboboxOptionEnum.检验通过.ToString()
            };
            ComboboxOption = ComboboxOptionEnum.所有.ToString();
        }

        private DataElementData AllElements { get; set; }
        public object SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }
        public DataElementData Elements
        {
            get => _elements;
            set => SetProperty(ref _elements, value);
        }
        public string ComboboxOption
        {
            get => _comboboxOption;
            set => SetProperty(ref _comboboxOption, value);
        }
        public ICommand LoadDataFromExcelCommand { get; }

        /// <summary>
        /// 从Excel文件加载数据,并检测模型
        /// </summary>
        private void LoadDataFromExcel()
        {
            List<ExcelRecord> records = ExcelDataProcessor.LoadDataFromExcel();
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
            Elements.Clear();
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
                            dataInstance.IsNameCorrect = false;
                        }
                        else
                        {
                            dataInstance.IsNameCorrect = true;
                            dataInstance.CheckParameters(record);
                        }
                    }
                    else
                    {
                        var extendName = element.GetExtendName();
                        var typeNames = systemFamilyDictionary.FindAll(a => CheckRecordExtendName(a, element)).ToList();
                        if (typeNames.Count() == 0 || !typeNames.Exists(a => a.TypeName == element.GetFamilyType()))
                        {
                            dataInstance.IsNameCorrect = false;
                        }
                        else
                        {
                            var record = typeNames.First(a => a.TypeName == element.GetFamilyType());
                            dataInstance.IsNameCorrect = true;
                            dataInstance.CheckParameters(record);
                        }
                    }
                }
            }
            ComboboxOption = ComboboxOptionEnum.所有.ToString();
            Elements = AllElements.Copy();
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
            Elements.Clear();
            switch (ComboboxOption)
            {
                case "所有":
                    Elements = AllElements.Copy();
                    break;
                case "族匹配缺失":
                    Elements = AllElements.FindFamilyNameIncorrect();
                    break;
                case "属性项匹配缺失":
                    Elements = AllElements.FindParameterIncorrect();
                    break;
                case "检验通过":
                    Elements = AllElements.FindCorrect();
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
                case DataInstance dataInstance:
                    SelectElementInRevit(dataInstance);
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
                foreach (var dataInstance in familyType.FamilyExtends)
                {
                    elementIds.Add(dataInstance.Element.Id);
                }
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(Models.FamilyType familyType)
        {
            var uidoc = _uiDocument;
            var elementIds = new List<ElementId>();
            foreach (var dataInstance in familyType.FamilyExtends)
            {
                elementIds.Add  (dataInstance.Element.Id);
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(DataInstance dataInstance)
        {
            SelectElementInRevit(dataInstance.Element);
        }
        private void SelectElementInRevit(Element element)
        {
            var uidoc = _uiDocument;
            var elementId = element.Id;
            uidoc.Selection.SetElementIds(new List<ElementId> { elementId });
        }
    }
    public enum ComboboxOptionEnum
    {
        所有,
        族匹配缺失,
        属性项匹配缺失,
        检验通过
    }
}
