using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using OfficeOpenXml;
using RZData.Helper;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RZData.ViewModels.RevitDataCheckViewModel;

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
        public ViewModelLocator(UIDocument _uiDocument)
        {
            AllElements = new DataElement();
            FamilyNameCheckElements = new DataElement();
            ParametersCheckElements = new DataElement();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            this.UiDocument = _uiDocument;
            RevitTemplateLoadViewModel = new RevitTemplateLoadViewModel.RevitTemplateLoadViewModel();
            LoadAllRevitElements();
            RevitDataCheckViewModel = new RevitDataCheckViewModel.RevitDataCheckViewModel(_uiDocument, this);
            RevitDataEntryViewModel = new RevitDataEntryViewModel.RevitDataEntryViewModel(_uiDocument, AllElements);
            RevitListSummaryViewModel = new RevitListSummaryViewModel(_uiDocument, AllElements);
        }

        public RevitTemplateLoadViewModel.RevitTemplateLoadViewModel RevitTemplateLoadViewModel { get; }
        public RevitDataEntryViewModel.RevitDataEntryViewModel RevitDataEntryViewModel { get; set; }
        public RevitDataCheckViewModel.RevitDataCheckViewModel RevitDataCheckViewModel { get; set; }
        public RevitListSummaryViewModel RevitListSummaryViewModel { get; set; }

        public void Reset()
        {
            LoadAllRevitElements();
            RevitDataCheckViewModel = new RevitDataCheckViewModel.RevitDataCheckViewModel(UiDocument, this);
            RevitDataEntryViewModel = new RevitDataEntryViewModel.RevitDataEntryViewModel(UiDocument, AllElements);
            RevitListSummaryViewModel = new RevitListSummaryViewModel(UiDocument, AllElements);
        }

        public void LoadAllRevitElements()
        {
            List<ExcelFamilyRecord> records = ExcelDataHelper.ExcelFamilyRecords;
            //表格中以MIC开头的族为可加载族，其他为系统族
            var systemFamilyDictionary = records.FindAll(a => !a.FamilyName.StartsWith("MIC"));
            var loadableFamilyDictionary = records.FindAll(a => a.FamilyName.StartsWith("MIC"));
            var familyList = new List<string>();
            records.ForEach(a => { if (!familyList.Contains(a.FamilyCategory)) familyList.Add(a.FamilyCategory); });
            var document = UiDocument.Document;
            var collector = new FilteredElementCollector(document);
            var elements = collector.WhereElementIsNotElementType();

            AllElements.Clear();
            FamilyNameCheckElements.Clear();
            ParametersCheckElements.Clear();
            DataElement revitElementData = new DataElement();

            foreach (var element in elements)
            {
                if (familyList.Contains(element.GetFamilyCategory()))
                {
                    var dataInstance = AllElements.Add(element);
                    if (element is FamilyInstance familyInstance)
                    {
                        ProcessFamilyInstance(loadableFamilyDictionary, document, element, dataInstance);
                    }
                    else
                    {
                        ProcessNonFamilyInstance(systemFamilyDictionary, document, element, dataInstance);
                    }
                }
            }
            AllElements.MergeParameters();
            FamilyNameCheckElements.MergeParameters();
            ParametersCheckElements.MergeParameters();
        }
        private void ProcessNonFamilyInstance(List<ExcelFamilyRecord> systemFamilyDictionary, Document document, Element element, DataInstance dataInstance)
        {
            var extendName = element.GetExtendName();
            var typeNames = systemFamilyDictionary.FindAll(a => CheckRecordExtendName(a, element)).ToList();
            if (typeNames.Count() == 0 || !typeNames.Exists(a => a.FamilyName == element.GetFamily()))
            {
                dataInstance.FamilyExtend.IsNameCorrect = false;
                FamilyNameCheckElements.Add(dataInstance);
            }
            else
            {
                var record = typeNames.First(a => a.FamilyName == element.GetFamily());
                dataInstance.FamilyExtend.IsNameCorrect = true;
                if (!dataInstance.CheckParameters(record, document))
                {
                    ParametersCheckElements.Add(dataInstance);
                }
            }
        }
        private void ProcessFamilyInstance(List<ExcelFamilyRecord> loadableFamilyDictionary, Document document, Element element, DataInstance dataInstance)
        {
            var typeName = element.GetFamily();
            var record = loadableFamilyDictionary.FirstOrDefault(a => typeName.StartsWith(a.FamilyName.Substring(0, a.FamilyName.Length - 1)));
            if (record == null || element.GetFamilyCategory() != record.FamilyCategory)
            {
                dataInstance.FamilyExtend.IsNameCorrect = false;
                FamilyNameCheckElements.Add(dataInstance);
            }
            else
            {
                dataInstance.FamilyExtend.IsNameCorrect = true;
                if (!dataInstance.CheckParameters(record, document))
                {
                    ParametersCheckElements.Add(dataInstance);
                }
            }
        }
        private bool CheckRecordExtendName(ExcelFamilyRecord excelRecord, Element element)
        {
            const string typePrefix = "类型=";
            var recordExtendName = excelRecord.ExtendName;
            string incorrectMessage = $"补充属性不合理，族：{excelRecord.FamilyCategory} 类型：{excelRecord.FamilyName} 补充属性：{excelRecord.ExtendName}";
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
                            TaskDialog.Show("错误信息", incorrectMessage);
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
                    return element.GetExtendName().StartsWith(recordExtendName.Substring(3, recordExtendName.Length - 4));
                }
                else { TaskDialog.Show("错误信息", incorrectMessage); return false; }
            }
        }
    }
}
