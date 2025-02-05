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
        public RevitTemplateLoadViewModel()
        {
            AllElements = new DataElement();
            FamilyNameCheckElements = new DataElement();
            ParametersCheckElements = new DataElement();

            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            OKCommand = new RelayCommand(OK);

            LoadFileName = string.IsNullOrEmpty(Path.GetFileName(LoadTemplatePath)) ? "未选中文件" : Path.GetFileName(LoadTemplatePath);
            CurrentFileName = string.IsNullOrEmpty(Path.GetFileName(CurrentTemplatePath)) ? "无" : Path.GetFileName(CurrentTemplatePath);
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
        public Action CloseAction { get; set; }

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
                    CloseAction?.Invoke();
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("错误信息", e.Message);
            }
        }
    }
}
