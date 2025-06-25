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
        private ObservableCollection<(string, string)> _requiredProperties;
        private string _selectedFilterProperty;
        private ObservableCollection<FilterConditionViewModel> _filterConditions;
        private string _selectedFilterValue;


        //选择的过滤的值
        public string SelectedFilterValue { get => _selectedFilterValue; set => SetProperty(ref _selectedFilterValue, value); }
        /// <summary>
        /// 过滤条件集合
        /// </summary>
        public ObservableCollection<FilterConditionViewModel> FilterConditions { get => _filterConditions; set => SetProperty(ref _filterConditions, value); }
        /// <summary>
        /// 筛选器中选中的筛选属性
        /// </summary>
        public string SelectedFilterProperty { get => _selectedFilterProperty; set => SetProperty(ref _selectedFilterProperty, value); }

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
        public ObservableCollection<(string, string)> RequiredProperties
        {
            get => _requiredProperties;
            set => SetProperty(ref _requiredProperties, value);
        }

        public ICommand OKWitheRequiredPropertiesCommand { get; }
        public ICommand DeleteRequiredPropertyCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand AddFilterConditionCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand RemoveFilterConditionCommand { get; }
        public RevitListSummaryViewModel(UIDocument uiDocument, ObservableCollection<RevitSolidElement> solidElements)
        {
            UiDocument = uiDocument;
            AllElements = new ElementViewModel(solidElements.ToList());
            AllMaterialList = new ObservableCollection<MaterialViewModel>();
            ShowMaterialList = new ObservableCollection<MaterialViewModel>();
            ShowAssemblyList = new ObservableCollection<AssemblyViewModel>();
            UnmatchedAssemblyList = new ObservableCollection<AssemblyViewModel>();
            FilterConditions = new ObservableCollection<FilterConditionViewModel>();
            PropertyNames = new ObservableCollection<string>();
            PropertyValues = new ObservableCollection<string>();
            RequiredProperties = new ObservableCollection<(string, string)>();

            OKWitheRequiredPropertiesCommand = new RelayCommand(OKWithRequiredProperties);
            DeleteRequiredPropertyCommand = new RelayCommand<(string, string)>(DeleteRequiredProperty);
            ExportExcelCommand = new RelayCommand(ExportExcel);
            AddFilterConditionCommand = new RelayCommand(AddFilterCondition);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            RemoveFilterConditionCommand = new RelayCommand<FilterConditionViewModel>(RemoveFilterCondition);
        }

        private void RemoveFilterCondition(FilterConditionViewModel condition)
        {
            // condition 参数就是 CommandParameter 传递过来的数据
            if (condition != null)
            {
                // 从集合中移除这个筛选条件
                FilterConditions.Remove(condition);
            }
            //移除一个筛选条件后要对所有的条件从新进行筛选
            ShowMaterialList = AllMaterialList;
            try
            {
                ShowMaterialList = new ObservableCollection<MaterialViewModel>(ShowMaterialList.ToList().FindAll(a => FilterTool.FilterMaterialList(a, FilterConditions.ToList())).ToList());
            }
            catch
            {
                TaskDialog.Show("错误信息", "存在筛选条件不合法，将清空筛选项。");
                FilterConditions = new ObservableCollection<FilterConditionViewModel>();
                return;
            }
        }

        private void ClearFilter()
        {
            FilterConditions = new ObservableCollection<FilterConditionViewModel>();
            ShowMaterialList = AllMaterialList;
        }

        private void AddFilterCondition()
        {
            try
            {
                FilterConditionViewModel filterConditionViewModel = new FilterConditionViewModel()
                {
                    PropertyName = SelectedFilterProperty,
                    PropertyValue = SelectedFilterValue,
                    Logic = "等于"
                };
                //只对新增筛选条件筛选
                var d = ShowMaterialList.ToList().FindAll(a => FilterTool.FilterMaterialList(a, filterConditionViewModel)).ToList();
                ShowMaterialList = new ObservableCollection<MaterialViewModel>(ShowMaterialList.ToList().FindAll(a => FilterTool.FilterMaterialList(a, filterConditionViewModel)).ToList());
                FilterConditions.Add(filterConditionViewModel);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
                return;
            }
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
                case ConstString.MaterialName:
                    return materialRecord.MaterialName == required.Item2;
                case ConstString.UsageMethod:
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

        public void GetMaterialListFromDataElement()
        {
            try
            {
                var list = AllElements.RevitSolidElements;
                AllMaterialList = PropertyMatchTool.FillMaterialList(UiDocument, list, out List<int> noMatchedElement);
                foreach (var elementID in noMatchedElement)
                {
                    Element element = UiDocument.Document.GetElement(new ElementId(elementID));
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
                PropertyNames.Add(ConstString.MaterialName);
                PropertyNames.Add(ConstString.UsageMethod);
                PropertyNames.Add(ConstString.RoomName);
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
                if (string.IsNullOrEmpty(SelectedFilterProperty))
                {
                    return;
                }
                switch (SelectedFilterProperty)
                {
                    case ConstString.MaterialName:
                        foreach (var materialViewModel in ShowMaterialList)
                        {
                            if (!PropertyValues.Contains(materialViewModel.MaterialName))
                            {
                                PropertyValues.Add(materialViewModel.MaterialName);
                            }
                        }
                        break;
                    case ConstString.UsageMethod:
                        foreach (var materialViewModel in ShowMaterialList)
                        {
                            if (!PropertyValues.Contains(materialViewModel.UsageMethod))
                            {
                                PropertyValues.Add(materialViewModel.UsageMethod);
                            }
                        }
                        break;
                    case ConstString.RoomName:
                        foreach (var materialViewModel in ShowMaterialList)
                        {
                            if (!PropertyValues.Contains(materialViewModel.Room))
                            {
                                PropertyValues.Add(materialViewModel.Room);
                            }
                        }
                        break;
                    default:
                        foreach (var materialViewModel in ShowMaterialList)
                        {
                            foreach (var feature in materialViewModel.ProjectFeaturesDetail)
                            {
                                if (SelectedFilterProperty == feature.Key && !PropertyValues.Contains(feature.Value))
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
