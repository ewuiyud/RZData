using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Navigation.Regions;
using RZData.Services;
using System;
using System.IO;
using System.Windows.Input;

namespace RZData.ViewModels
{
    public class RevitTemplateLoadViewModel : ObservableObject
    {
        public RevitTemplateLoadViewModel()
        {
            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            OKCommand = new RelayCommand(OK);
            LoadFileName = string.IsNullOrEmpty(Path.GetFileName(LoadTemplatePath)) ? "未选中文件" : Path.GetFileName(LoadTemplatePath);
            CurrentFileName = string.IsNullOrEmpty(Path.GetFileName(CurrentTemplatePath)) ? "无" : Path.GetFileName(CurrentTemplatePath);
        }
        private string currentTemplatePath;
        private string loadTemplatePath;
        private string currentFileName;
        private string loadFileName;
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
        /// <summary>
        /// View中绑定的属性
        /// </summary>
        public string CurrentFileName
        {
            get => currentFileName;
            set => SetProperty(ref currentFileName, value);
        }
        /// <summary>
        /// View中绑定的属性
        /// </summary>
        public string LoadFileName
        {
            get => loadFileName;
            set => SetProperty(ref loadFileName, value);
        }
        public ICommand LoadDataFromExcelCommand { get; }
        public ICommand OKCommand { get; }
        public Action CloseAction { get; set; }
        public IRegionManager RegionManager { get; }

        private void LoadDataFromExcel()
        {
            try
            {
                string path = ExcelDataService.LoadDataFromExcel();
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
                    ExcelDataService.GetContent(LoadTemplatePath);
                    CurrentTemplatePath = LoadTemplatePath;
                    LoadTemplatePath = "";
                    CurrentFileName = loadFileName;
                    loadFileName = "无";
                    CloseAction?.Invoke();
                }
                else
                {
                    TaskDialog.Show("提示", "请选择模板!");
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("错误信息", "表格加载失败，错误信息：" + e.Message);
            }
        }
    }
}
