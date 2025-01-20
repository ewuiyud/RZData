using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RZData.Helper;
using RZData.Models;
using RZData.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace RZData.ViewModels
{
    public class RevitTemplateLoadViewModel : BaseViewModel
    {
        private RevitTemplateLoadView view;
        public RevitTemplateLoadViewModel(UIDocument uiDocument)
        {
            AllElements = new DataElement();
            FamilyNameCheckElements = new DataElement();
            ParametersCheckElements = new DataElement();

            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            OKCommand = new RelayCommand(OK);

            LoadFileName = string.IsNullOrEmpty(Path.GetFileName(LoadTemplatePath)) ? "未选中文件" : Path.GetFileName(LoadTemplatePath);
            CurrentFileName = string.IsNullOrEmpty(Path.GetFileName(CurrentTemplatePath)) ? "无" : Path.GetFileName(CurrentTemplatePath);
            UiDocument = uiDocument;
        }
        private string currentTemplatePath;
        private string loadTemplatePath;
        private string currentFileName;
        private string loadFileName;

        private List<ExcelFamilyRecord> records;
        public string LoadTemplatePath
        {
            get => loadTemplatePath;
            set => SetProperty(ref loadTemplatePath, value);
        }
        public string CurrentTemplatePath
        {
            get => currentTemplatePath;
            set => SetProperty(ref currentTemplatePath, value);
        }
        public List<ExcelFamilyRecord> Records
        {
            get => records;
            set => SetProperty(ref records, value);
        }
        public string CurrentFileName
        {
            get => currentFileName;
            set => SetProperty(ref currentFileName, value);
        }
        public string LoadFileName
        {
            get => loadFileName;
            set => SetProperty(ref loadFileName, value);
        }
        public ICommand LoadDataFromExcelCommand { get; }
        public ICommand OKCommand { get; }
        public void SetView(RevitTemplateLoadView view)
        {
            this.view = view;
        }
        private void LoadDataFromExcel()
        {
            try
            {
                string path = ExcelDataHelper.LoadDataFromExcel();
                if (path != null)
                {
                    LoadTemplatePath = path;
                    LoadFileName = string.IsNullOrEmpty(Path.GetFileName(LoadTemplatePath)) ? "未选中文件" : Path.GetFileName(LoadTemplatePath);
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("错误信息", e.Message);
            }
        }
        private void OK()
        {
            try
            {
                if (!string.IsNullOrEmpty(LoadTemplatePath)) 
                {
                    ExcelDataHelper.GetContent(LoadTemplatePath);
                    CurrentTemplatePath = LoadTemplatePath;
                    LoadTemplatePath = "";
                    CurrentFileName = loadFileName;
                    loadFileName = "无";
                    CheckModel(ExcelDataHelper.ExcelFamilyRecords);
                    ViewModelLocator.Instance(UiDocument).Reset(this);
                    view.Close();
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("错误信息", e.Message);
            }
        }

        public void CheckModel(List<ExcelFamilyRecord> records)
        {
            var systemFamilyDictionary = records.FindAll(a => !a.TypeName.StartsWith("MIC"));
            var loadableFamilyDictionary = records.FindAll(a => a.TypeName.StartsWith("MIC"));
            var familyList = new List<string>();
            records.ForEach(a => { if (!familyList.Contains(a.FamilyName)) familyList.Add(a.FamilyName); });
            var document = UiDocument.Document;
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
