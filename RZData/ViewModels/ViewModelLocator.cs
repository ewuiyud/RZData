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

namespace RZData.ViewModels
{
    public class ViewModelLocator : BaseViewModel
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
            AllElements = new DataElement();
            FamilyNameCheckElements = new DataElement();
            ParametersCheckElements = new DataElement();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            this._uiDocument = _uiDocument;
            RevitTemplateLoadViewModel = new RevitTemplateLoadViewModel(_uiDocument);
            LoadAllRevitElements();
            RevitDataCheckViewModel = new RevitDataCheckViewModel(_uiDocument, this);
            RevitDataEntryViewModel = new RevitDataEntryViewModel(_uiDocument, AllElements);
            RevitListSummaryViewModel = new RevitListSummaryViewModel(_uiDocument, AllElements);
        }

        public RevitTemplateLoadViewModel RevitTemplateLoadViewModel { get; }
        public RevitDataEntryViewModel RevitDataEntryViewModel { get; set; }
        public RevitDataCheckViewModel RevitDataCheckViewModel { get; set; }
        public RevitListSummaryViewModel RevitListSummaryViewModel { get; set; }

        public void Reset()
        {
            LoadAllRevitElements();
            RevitDataCheckViewModel = new RevitDataCheckViewModel(_uiDocument, this);
            RevitDataEntryViewModel = new RevitDataEntryViewModel(_uiDocument, AllElements);
            RevitListSummaryViewModel = new RevitListSummaryViewModel(_uiDocument, AllElements);
        }

        public void LoadAllRevitElements()
        {
            List<ExcelFamilyRecord> records = ExcelDataHelper.ExcelFamilyRecords;
            var systemFamilyDictionary = records.FindAll(a => !a.TypeName.StartsWith("MIC"));
            var loadableFamilyDictionary = records.FindAll(a => a.TypeName.StartsWith("MIC"));
            var familyList = new List<string>();
            records.ForEach(a => { if (!familyList.Contains(a.FamilyName)) familyList.Add(a.FamilyName); });
            var document = _uiDocument.Document;
            var collector = new FilteredElementCollector(document);
            var elements = collector.WhereElementIsNotElementType();

            AllElements.Clear();
            FamilyNameCheckElements.Clear();
            ParametersCheckElements.Clear();
            DataElement revitElementData = new DataElement();

            foreach (var element in elements)
            {
                if (familyList.Contains(element.GetFamily()))
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
        private void ProcessFamilyInstance(List<ExcelFamilyRecord> loadableFamilyDictionary, Document document, Element element, DataInstance dataInstance)
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
            string incorrectMessage = $"补充属性不合理，族：{excelRecord.FamilyName} 类型：{excelRecord.TypeName} 补充属性：{excelRecord.ExtendName}";
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
