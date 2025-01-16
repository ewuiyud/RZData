using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RZData.Helper;
using RZData.Models;
using RZData.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace RZData.ViewModels
{
    public class RevitTemplateLoadViewModel : BaseViewModel
    {
        private RevitTemplateLoadView view;
        private UIDocument _uiDocument;
        public RevitTemplateLoadViewModel(UIDocument uiDocument)
        {
            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            OKCommand = new RelayCommand(OK);
            LoadFileName = string.IsNullOrEmpty(Path.GetFileName(LoadTemplatePath)) ? "未选中文件" : Path.GetFileName(LoadTemplatePath);
            CurrentFileName = string.IsNullOrEmpty(Path.GetFileName(CurrentTemplatePath)) ? "无" : Path.GetFileName(CurrentTemplatePath);
            _uiDocument = uiDocument;
        }
        private string currentTemplatePath;
        private string loadTemplatePath;
        private string currentFileName;
        private string loadFileName;

        private List<ExcelRecord> records;
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
        public List<ExcelRecord> Records
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
            string path = "";
            records = ExcelDataProcessor.LoadDataFromExcel(ref path);
            LoadTemplatePath = path;
            LoadFileName = string.IsNullOrEmpty(Path.GetFileName(LoadTemplatePath)) ? "未选中文件" : Path.GetFileName(LoadTemplatePath);
        }
        private void OK()
        {
            if (!string.IsNullOrEmpty(LoadTemplatePath))
            {
                CurrentTemplatePath = LoadTemplatePath;
                LoadTemplatePath = "";
                CurrentFileName = loadFileName;
                loadFileName = "无";
                RevitDataCheckViewModel.Instance().CheckModel(records);
                view.Close();
            }
        }

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
                            if (!dataInstance.CheckParameters(record, document))
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
        }
    }
}
