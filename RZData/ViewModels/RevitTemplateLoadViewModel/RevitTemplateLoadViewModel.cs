using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RZData.ExternalEventHandlers;
using RZData.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RZData.ViewModels
{
    public class RevitTemplateLoadViewModel : ObservableObject
    {
        public RevitTemplateLoadViewModel(UIDocument uIDocument)
        {
            this.uIDocument = uIDocument;
            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            LoadParametersCommand = new AsyncRelayCommand(LoadParameters);
            OKCommand = new AsyncRelayCommand(OK);
            LoadFileName = string.IsNullOrEmpty(Path.GetFileName(LoadTemplatePath)) ? "未选中文件" : Path.GetFileName(LoadTemplatePath);
            CurrentFileName = string.IsNullOrEmpty(Path.GetFileName(CurrentTemplatePath)) ? "无" : Path.GetFileName(CurrentTemplatePath);
        }

        private readonly UIDocument uIDocument;
        private string currentTemplatePath;
        private string loadTemplatePath;
        private string currentFileName;
        private string loadFileName;
        private bool isLoadParameters;
        public bool IsLoadParameters { get => isLoadParameters; set => SetProperty(ref isLoadParameters, value); }
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
        public ICommand LoadParametersCommand { get; }
        public Action CloseAction { get; set; }

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
        //录入参数的方法
        private async Task LoadParameters()
        {
            try
            {
                var result = TaskDialog.Show("提示", "是否确定同时录入参数。", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel);
                if (result != TaskDialogResult.Ok)
                {
                    return;
                }
                var uiDoc = uIDocument;
                var doc = uiDoc.Document;

                // 处理Excel数据
                var excelProcessor = new ExcelDataProcessorForParameters();
                var familyData = excelProcessor.ProcessExcelFile(LoadTemplatePath);

                if (!excelProcessor.ValidateData(familyData))
                {
                    TaskDialog.Show("错误", "Excel数据验证失败，请检查数据格式");
                    return;
                }

                // 执行Revit属性操作
                var propertyManager = new RevitPropertyManager(uiDoc);
                await CustomHandler.Run(a =>
                {
                    propertyManager.AddPropertiesToFamilies(familyData);
                });

                // 显示成功信息
                var instanceCounts = propertyManager.GetFamilyInstanceCounts();
                var successMessage = $"成功添加了 {familyData.Count} 个参数。\n\n";
                TaskDialog.Show("成功", successMessage);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误", $"操作失败: {ex.Message}");
            }
        }
        private async Task OK()
        {
            try
            {
                if (!string.IsNullOrEmpty(LoadTemplatePath))
                {
                    ExcelDataService.GetContent(LoadTemplatePath);
                    CurrentTemplatePath = LoadTemplatePath;
                    if (isLoadParameters) { await LoadParameters(); isLoadParameters = false; }
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
