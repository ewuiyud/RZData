using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RZData.Models;
using RZData.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RZData.ViewModels
{
    public class RevitTemplateLoadViewModel : ObservableObject
    {
        private RevitTemplateLoadView view;
        public RevitTemplateLoadViewModel()
        {
            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            OKCommand = new RelayCommand(OK);
            LoadFileName = string.IsNullOrEmpty(Path.GetFileName(LoadTemplatePath)) ? "未选中文件" : Path.GetFileName(LoadTemplatePath);
            CurrentFileName = string.IsNullOrEmpty(Path.GetFileName(CurrentTemplatePath)) ? "无" : Path.GetFileName(CurrentTemplatePath);
        }
        public RevitTemplateLoadViewModel(RevitTemplateLoadView view)
        {
            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            OKCommand = new RelayCommand(OK);
            this.view = view;
            LoadFileName = string.IsNullOrEmpty(Path.GetFileName(LoadTemplatePath)) ? "未选中文件" : Path.GetFileName(LoadTemplatePath);
            CurrentFileName = string.IsNullOrEmpty(Path.GetFileName(CurrentTemplatePath)) ? "无" : Path.GetFileName(CurrentTemplatePath);
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
            CurrentTemplatePath = LoadTemplatePath;
            LoadTemplatePath = "";
            CurrentFileName = loadFileName;
            loadFileName = "无";
            view.Close();
        }
    }
}
