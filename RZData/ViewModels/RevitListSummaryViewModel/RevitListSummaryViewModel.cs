using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Input;
using RZData.Extensions;
using RZData.Models;
using RZData.Services;
using RZData.Tools;
using RZData.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace RZData.ViewModels
{
    public class RevitListSummaryViewModel : BaseViewModel
    {
        private ObservableCollection<MaterialViewModel> _allMaterialList;
        private ObservableCollection<MaterialViewModel> _showMaterialList;
        private MaterialViewModel _selectedMaterialRecord;
        private ObservableCollection<AssemblyViewModel> _showAssemblyList;
        private ObservableCollection<AssemblyViewModel> _unmatchedAssemblyList;
        private AssemblyViewModel _selectedAssemblyRecord;
        private AssemblyViewModel _selectedUnMatchedAssemblyRecord;
        private ObservableCollection<string> _propertyNames;
        private ObservableCollection<string> _propertyValues;
        private string _selectedPropertyName;
        private string _selectedPropertyValue;
        private ObservableCollection<(string, string)> _requiredProperties;


        public ObservableCollection<MaterialViewModel> AllMaterialList
        {
            get => _allMaterialList;
            set => SetProperty(ref _allMaterialList, value);
        }
        public ObservableCollection<MaterialViewModel> ShowMaterialList
        {
            get => _showMaterialList;
            set => SetProperty(ref _showMaterialList, value);
        }
        public MaterialViewModel SelectedMaterialRecord
        {
            get => _selectedMaterialRecord;
            set => SetProperty(ref _selectedMaterialRecord, value);
        }
        public AssemblyViewModel SelectedAssemblyRecord
        {
            get => _selectedAssemblyRecord;
            set => SetProperty(ref _selectedAssemblyRecord, value);
        }
        public AssemblyViewModel SelectedUnMatchedAssemblyRecord
        {
            get => _selectedUnMatchedAssemblyRecord;
            set => SetProperty(ref _selectedUnMatchedAssemblyRecord, value);
        }
        /// <summary>
        /// 构件细项表绑定数据
        /// </summary>
        public ObservableCollection<AssemblyViewModel> ShowAssemblyList
        {
            get => _showAssemblyList;
            set => SetProperty(ref _showAssemblyList, value);
        }
        /// <summary>
        /// 未匹配元素表绑定数据
        /// </summary>
        public ObservableCollection<AssemblyViewModel> UnmatchedAssemblyList
        {
            get => _unmatchedAssemblyList;
            set => SetProperty(ref _unmatchedAssemblyList, value);
        }
        public ObservableCollection<string> PropertyNames
        {
            get => _propertyNames;
            set => SetProperty(ref _propertyNames, value);
        }
        public ObservableCollection<string> PropertyValues
        {
            get => _propertyValues;
            set => SetProperty(ref _propertyValues, value);
        }
        public string SelectedPropertyName
        {
            get => _selectedPropertyName;
            set => SetProperty(ref _selectedPropertyName, value);
        }
        public string SelectedPropertyValue
        {
            get => _selectedPropertyValue;
            set => SetProperty(ref _selectedPropertyValue, value);
        }
        public ObservableCollection<(string, string)> RequiredProperties
        {
            get => _requiredProperties;
            set => SetProperty(ref _requiredProperties, value);
        }

        public ICommand CansoleCommand { get; }
        public ICommand AddRequiredPropertiesCommand { get; }
        public ICommand DeleteRequiredPropertiesCommand { get; }
        public ICommand OKWitheRequiredPropertiesCommand { get; }
        public ICommand DeleteRequiredPropertyCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public RevitListSummaryViewModel(UIDocument uiDocument, ObservableCollection<RevitSolidElement> solidElements)
        {
            UiDocument = uiDocument;
            AllElements = new ElementViewModel(solidElements.ToList());
            AllMaterialList = new ObservableCollection<MaterialViewModel>();
            ShowMaterialList = new ObservableCollection<MaterialViewModel>();
            ShowAssemblyList = new ObservableCollection<AssemblyViewModel>();
            UnmatchedAssemblyList = new ObservableCollection<AssemblyViewModel>();
            PropertyNames = new ObservableCollection<string>();
            PropertyValues = new ObservableCollection<string>();
            RequiredProperties = new ObservableCollection<(string, string)>();
            AddRequiredPropertiesCommand = new RelayCommand(AddRequiredProperties);
            DeleteRequiredPropertiesCommand = new RelayCommand(DeleteRequiredProperties);
            OKWitheRequiredPropertiesCommand = new RelayCommand(OKWithRequiredProperties);
            DeleteRequiredPropertyCommand = new RelayCommand<(string, string)>(DeleteRequiredProperty);
            ExportExcelCommand = new RelayCommand(ExportExcel);
        }

        private void ExportExcel()
        {
            try
            {
                ExcelDataService.ExportToExcelFromMaterialList(ShowMaterialList);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void OKWithRequiredProperties()
        {
            try
            {
                ObservableCollection<MaterialViewModel> temp = new ObservableCollection<MaterialViewModel>();
                if (ShowMaterialList.Count == 0)
                {
                    ShowMaterialList = AllMaterialList;
                }
                foreach (var materialRecord in AllMaterialList)
                {
                    if (RequiredProperties.ToList().All(a => MatchRequired(a, materialRecord)))
                        temp.Add(materialRecord);
                }
                ShowMaterialList = temp;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private bool MatchRequired((string, string) required, MaterialViewModel materialRecord)
        {
            switch (required.Item1)
            {
                case "材料名称":
                    return materialRecord.MaterialName == required.Item2;
                case "使用方式":
                    return materialRecord.UsageMethod == required.Item2;
                default:
                    foreach (var feature in materialRecord.ProjectFeaturesDetail)
                    {
                        if (required.Item1 == feature.Key)
                            if (string.IsNullOrEmpty(required.Item2))
                            {
                                return true; //如果筛选项仅有名称，则只要有词条属性都可以通过筛选。
                            }
                        if (feature.Value == required.Item2)
                        {
                            return true;
                        }
                    }
                    return false;
            }
        }

        private void DeleteRequiredProperties()
        {
            try
            {
                RequiredProperties.Clear();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void AddRequiredProperties()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedPropertyName))
                {
                    return;
                }
                RequiredProperties.Add((SelectedPropertyName, SelectedPropertyValue));
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        public void GetMaterialListFromDataElement()
        {
            try
            {
                var list = AllElements.RevitSolidElements;
                AllMaterialList = PropertyMatchTool.FillMaterialList(UiDocument, list, out List<Element> noMatchedElement);
                foreach (var element in noMatchedElement)
                {
                    UnmatchedAssemblyList.Add(new AssemblyViewModel()
                    {
                        AssemblyID = element.Id.ToString(),
                        AssemblyName = element.LookupParameter("族与类型").AsValueString(),
                        //Modelbelonging = element.Document == UiDocument.Document ? "当前模型" : "链接模型"
                    });
                }
                ShowMaterialList = AllMaterialList;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        internal void GetAssemblyList()
        {
            try
            {
                ShowAssemblyList = new ObservableCollection<AssemblyViewModel>();
                if (SelectedMaterialRecord != null)
                    foreach (var revitSolidElement in SelectedMaterialRecord.RevitSolidElements)
                    {
                        var element = UiDocument.Document.GetElement(new ElementId(revitSolidElement.ID));
                        ShowAssemblyList.Add(new AssemblyViewModel()
                        {
                            AssemblyID = element.Id.ToString(),
                            AssemblyName = element.LookupParameter("族与类型").AsValueString(),
                            //Modelbelonging = element.Document == UiDocument.Document ? "当前模型" : "链接模型"
                        });
                    }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        internal void PropertyNameDroped()
        {
            try
            {
                PropertyNames = new ObservableCollection<string>();
                foreach (var materialRecord in AllMaterialList)
                {
                    foreach (var feature in materialRecord.ProjectFeaturesDetail)
                    {
                        if (!PropertyNames.Contains(feature.Key))
                        {
                            PropertyNames.Add(feature.Key);
                        }
                    }
                }
                PropertyNames.Add("材料名称");
                PropertyNames.Add("使用方式");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        internal void PropertyValueDroped()
        {
            try
            {
                PropertyValues = new ObservableCollection<string>();
                if (string.IsNullOrEmpty(SelectedPropertyName))
                {
                    return;
                }
                switch (SelectedPropertyName)
                {
                    case "材料名称":
                        foreach (var materialRecord in AllMaterialList)
                        {
                            if (!PropertyValues.Contains(materialRecord.MaterialName))
                            {
                                PropertyValues.Add(materialRecord.MaterialName);
                            }
                        }
                        break;
                    case "使用方式":
                        foreach (var materialRecord in AllMaterialList)
                        {
                            if (!PropertyValues.Contains(materialRecord.UsageMethod))
                            {
                                PropertyValues.Add(materialRecord.UsageMethod);
                            }
                        }
                        break;
                    default:
                        foreach (var materialRecord in AllMaterialList)
                        {
                            foreach (var feature in materialRecord.ProjectFeaturesDetail)
                            {
                                if (SelectedPropertyName == feature.Key && !PropertyValues.Contains(feature.Value))
                                {
                                    PropertyValues.Add(feature.Value);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        private void DeleteRequiredProperty((string, string) parameter)
        {
            if (RequiredProperties.Contains(parameter))
                RequiredProperties.Remove(parameter);
        }
        internal void DoubleClickAndPickObjects(bool selectAssemblyElement = true)
        {
            try
            {
                int id;
                if (selectAssemblyElement)
                {
                    id = int.Parse(SelectedAssemblyRecord.AssemblyID);
                }
                else
                {
                    id = int.Parse(SelectedUnMatchedAssemblyRecord.AssemblyID);
                }
                var uidoc = UiDocument;
                var elementIds = new List<ElementId>
                {
                    new ElementId(id)
                };
                uidoc.Selection.SetElementIds(elementIds);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
    }
}
