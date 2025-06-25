using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Input;
using RZData.ExternalEventHandlers;
using RZData.Models;
using RZData.Services;
using RZData.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Documents;
using System.Windows.Input;

namespace RZData.ViewModels
{

    public class RevitDataEntryViewModel : BaseViewModel
    {
        private object _selectedItem;
        private string _searchKeyword;
        private ElementViewModel _showElements;
        private FamilyCategoryViewModel _selectedElement;
        private ObservableCollection<int> _seletedElementID;
        private List<ParameterSetVM> _selectedItemParameters;
        private bool _showFilter;
        private bool _showProjectFeatures;
        private string _selectedFilterProperty;
        private List<string> _filterLogics = new List<string>() { "等于", "大于等于", "小于等于", "不等于", "正则表达式" };
        private ObservableCollection<FilterConditionViewModel> _filterConditions;
        private string _selectedFilterValue;
        private string _customFilterValue;
        private string _selectedFilterLogic = "等于";
        private bool _canAttachMaterialLibrary;
        private ExcelProductMaterialLibraryModel _selectedMaterialLibrary;
        public ExcelProductMaterialLibraryModel SelectedMaterialLibrary { get => _selectedMaterialLibrary; set => SetProperty(ref _selectedMaterialLibrary, value); }
        private ObservableCollection<ExcelProductMaterialLibraryModel> _availableMaterialLibraries;
        public ObservableCollection<ExcelProductMaterialLibraryModel> AvailableMaterialLibraries { get => _availableMaterialLibraries; set => SetProperty(ref _availableMaterialLibraries, value); }
        //能够挂接材料库
        public bool CanAttachMaterialLibrary { get => _canAttachMaterialLibrary; set => SetProperty(ref _canAttachMaterialLibrary, value); }
        //选择的过滤的逻辑
        public string SelectedFilterLogic { get => _selectedFilterLogic; set => SetProperty(ref _selectedFilterLogic, value); }
        //自定义的过滤的值
        public string CustomFilterValue { get => _customFilterValue; set => SetProperty(ref _customFilterValue, value); }
        //选择的过滤的值
        public string SelectedFilterValue { get => _selectedFilterValue; set => SetProperty(ref _selectedFilterValue, value); }
        /// <summary>
        /// 过滤条件集合
        /// </summary>
        public ObservableCollection<FilterConditionViewModel> FilterConditions { get => _filterConditions; set => SetProperty(ref _filterConditions, value); }
        /// <summary>
        /// 可用的过滤逻辑
        /// </summary>
        public List<string> FilterLogics { get => _filterLogics; set => SetProperty(ref _filterLogics, value); }
        /// <summary>
        /// 筛选器中选中的筛选属性
        /// </summary>
        public string SelectedFilterProperty { get => _selectedFilterProperty; set => SetProperty(ref _selectedFilterProperty, value); }
        public bool ShowProjectFeatures { get => _showProjectFeatures; set => SetProperty(ref _showProjectFeatures, value); }
        public bool ShowFilter { get => _showFilter; set => SetProperty(ref _showFilter, value); }
        /// <summary>
        /// 选中元素的参数集
        /// </summary>
        public List<ParameterSetVM> SelectedItemParameters { get => _selectedItemParameters; set => SetProperty(ref _selectedItemParameters, value); }
        public ICommand SearchCommand { get; }
        public ICommand SelectAllTreeItemsCommand { get; }
        public ICommand DeselectAllTreeItemsCommand { get; }
        public ICommand SelectInRevitCommand { get; }
        public ICommand EntryParametersCommand { get; }
        public ICommand OKCommand { get; }
        //双击触发
        public ICommand DoubleClickCommand { get; set; }
        //添加筛选条件
        public ICommand AddFilterConditionCommand { get; set; }
        //删除筛选条件
        public ICommand RemoveFilterConditionCommand { get; }
        //清空筛选条件
        public ICommand ClearFilterCommand { get; }
        //应用筛选条件
        public ICommand ApplyFilterCommand { get; }
        public ICommand ConfirmAttachCommand { get; }

        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }

        public ObservableCollection<int> SeletedElementID
        {
            get => _seletedElementID; set
            {
                SetProperty(ref _seletedElementID, value);
            }
        }
        private ObservableCollection<MaterialViewModel> _showMaterialList;
        /// <summary>
        /// 展示选中元素汇总的材料表
        /// </summary>
        public ObservableCollection<MaterialViewModel> ShowMaterialList { get => _showMaterialList; set => SetProperty(ref _showMaterialList, value); }
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                SetProperty(ref _searchKeyword, value);
                SearchCommand.Execute(null);
            }
        }
        public ElementViewModel ShowElements { get => _showElements; set => SetProperty(ref _showElements, value); }
        public ObservableCollection<FamilyCategoryViewModel> Families
        {
            get
            {
                var fs = new ObservableCollection<FamilyCategoryViewModel>
                {
                    new FamilyCategoryViewModel() { Name = "所有" }
                };
                var revitSolidElements = AllElements.RevitSolidElements.FindAll(a => FilterTool.FilterRevitElement(a, FilterConditions.ToList()));
                var curentElmentViewModel = new ElementViewModel(revitSolidElements);
                curentElmentViewModel.Children.ToList().ForEach(a => fs.Add(a));
                return fs;
            }
        }
        /// <summary>
        /// 选择族类别过滤，过滤逻辑在Set中
        /// </summary>
        public FamilyCategoryViewModel SelectedElement
        {
            get => _selectedElement;
            set
            {
                if (_selectedElement != value)
                {
                    _selectedElement = value;
                    OnPropertyChanged(nameof(SelectedElement));

                    ReShowElements();
                    var revitSolidElements = ShowElements.RevitSolidElements.FindAll(a => FilterTool.FilterRevitElement(a, FilterConditions.ToList()));
                    ShowElements = new ElementViewModel(revitSolidElements);
                }
            }
        }

        private void ReShowElements()
        {
            if (_selectedElement == null || _selectedElement.Name == "所有")
            {
                ShowElements = AllElements;
            }
            else
            {
                var revitSolidElements = AllElements.RevitSolidElements.ToList().FindAll(a => a.FamilyCategory == _selectedElement.Name);
                ShowElements = new ElementViewModel(revitSolidElements);
            }
            Search();
        }

        public RevitDataEntryViewModel(UIDocument _uiDocument, ObservableCollection<RevitSolidElement> revitSolidElements)
        {
            this.AllElements = new ElementViewModel(revitSolidElements.ToList().FindAll(a => a.IsNameCorrect).ToList());
            this.ShowElements = AllElements;
            this.UiDocument = _uiDocument;
            //集合初始化
            SeletedElementID = new ObservableCollection<int>();
            ShowMaterialList = new ObservableCollection<MaterialViewModel>();
            FilterConditions = new ObservableCollection<FilterConditionViewModel>();
            AvailableMaterialLibraries = new ObservableCollection<ExcelProductMaterialLibraryModel>();

            //命令初始化
            SearchCommand = new RelayCommand(Search);
            DoubleClickCommand = new RelayCommand(DoubleClick);
            SelectAllTreeItemsCommand = new RelayCommand(SelectAllTreeItems);
            DeselectAllTreeItemsCommand = new RelayCommand(DeselectAllTreeItems);
            SelectInRevitCommand = new RelayCommand(SelectInRevit);
            EntryParametersCommand = new RelayCommand(EntryParameters);
            AddFilterConditionCommand = new RelayCommand(AddFilterCondition);
            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            ConfirmAttachCommand = new AsyncRelayCommand(ConfirmAttachAsync);
            OKCommand = new AsyncRelayCommand(OK);
            RemoveFilterConditionCommand = new RelayCommand<FilterConditionViewModel>(RemoveFilter);

            ResetShowElements();
        }

        private async Task ConfirmAttachAsync()
        {
            var elements = ShowElements.GetAllElements().FindAll(a => a.IsChecked);
            var parameter = new ParameterSetVM()
            {
                Name = "关联材料库",
                Value = $"{SelectedMaterialLibrary.Name}-{SelectedMaterialLibrary.SerialNumber}",
                ValueType = "实例参数"
            };
            await CustomHandler.Run(a =>
            {
                SetParameter(a.ActiveUIDocument, parameter, elements);
            });
        }

        private void ApplyFilter()
        {
            SelectAllTreeItems();
            EntryParameters();
        }

        private void ClearFilter()
        {
            FilterConditions = new ObservableCollection<FilterConditionViewModel>();
            ReShowElements();
        }

        private void RemoveFilter(FilterConditionViewModel condition)
        {
            // condition 参数就是 CommandParameter 传递过来的数据
            if (condition != null)
            {
                // 从集合中移除这个筛选条件
                FilterConditions.Remove(condition);
            }
            //移除一个筛选条件后要对所有的条件从新进行筛选
            ReShowElements();
            try
            {
                var revitSolidElements = ShowElements.RevitSolidElements.ToList().FindAll(a => FilterTool.FilterRevitElement(a, FilterConditions.ToList()));
                ShowElements = new ElementViewModel(revitSolidElements);
            }
            catch
            {
                TaskDialog.Show("错误信息", "存在筛选条件不合法，将清空筛选项。");
                FilterConditions = new ObservableCollection<FilterConditionViewModel>();
                return;
            }
        }

        private void AddFilterCondition()
        {
            try
            {
                FilterConditionViewModel filterConditionViewModel = new FilterConditionViewModel()
                {
                    PropertyName = SelectedFilterProperty,
                    PropertyValue = string.IsNullOrEmpty(CustomFilterValue) ? SelectedFilterValue : CustomFilterValue,
                    Logic = SelectedFilterLogic
                };
                //只对新增筛选条件筛选
                var rvList = ShowElements.RevitSolidElements.ToList();
                var revitSolidElements = rvList.FindAll(a => FilterTool.FilterRevitElement(a, filterConditionViewModel));
                ShowElements = new ElementViewModel(revitSolidElements);
                FilterConditions.Add(filterConditionViewModel);

                ResetRightParametersList();
                ResetShowElements();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
                return;
            }
        }

        private void ResetRightParametersList()
        {
            SelectedFilterProperty = null;
            CustomFilterValue = null;
            SelectedFilterValue = null;
            ShowMaterialList = new ObservableCollection<MaterialViewModel>();
            SelectedMaterialLibrary = new ExcelProductMaterialLibraryModel();
            AvailableMaterialLibraries = new ObservableCollection<ExcelProductMaterialLibraryModel>();
            CanAttachMaterialLibrary = false;
        }

        private void DoubleClick()
        {
            AllElements.SelectAll(false);
            ShowElements.SelectAll(false);
            //选中该类别下的所有元素,并处理上级的选中状态
            ShowElements.SelectObject(SelectedItem);
            switch (SelectedItem)
            {
                case FamilyCategoryViewModel familyCategory:
                    SelectElementInRevit(familyCategory);
                    break;
                case FamilyViewModel family:
                    SelectElementInRevit(family);
                    break;
                case FamilyExtendViewModel familyExtend:
                    SelectElementInRevit(familyExtend);
                    break;
                case ElementInstanceViewModel elementInstance:
                    SelectElementInRevit(elementInstance);
                    break;
                default:
                    break;
            }
            EntryParameters();
        }

        //同步选中元素的参数
        private void EntryParameters()
        {
            //填充参数表
            SelectedItemParameters = new List<ParameterSetVM>();
            var elements = ShowElements.GetAllElements().FindAll(a => a.IsChecked);
            var eleParMin = elements.OrderBy(a => a.Parameters.Count).FirstOrDefault();
            if (eleParMin is null)
            {
                return;
            }
            foreach (var item in eleParMin.Parameters)
            {
                if (elements.All(a => a.Parameters.Exists(b => b.Name == item.Name)))
                {
                    var tempParameterSet = new ParameterSetVM(item);
                    elements.ForEach(a =>
                    {
                        var parameter = a.Parameters.FirstOrDefault(b => b.Name == item.Name);
                        if (parameter != null)
                        {
                            if (!tempParameterSet.Parameters.Contains(parameter))
                            {
                                tempParameterSet.Parameters.Add(parameter);
                            }
                        }
                    });
                    SelectedItemParameters.Add(tempParameterSet);
                }
            }
            //填充可挂接材料库选项
            GetAvailableMaterialLibraries();
            //填充项目特征表
            GetMaterialViewModels(elements);
        }
        /// <summary>
        /// 填充可挂接材料库选项
        /// </summary>
        private void GetAvailableMaterialLibraries()
        {
            CanAttachMaterialLibrary = false;
            //仅有唯一的产品名称时才可以关联材料库
            var p = SelectedItemParameters.FirstOrDefault(a => a.Name == "产品分类名称");
            if (p != null && !string.IsNullOrEmpty(p.Value) && p.Status != "多参数")
            {
                CanAttachMaterialLibrary = true;
                var productName = p.Value;
                AvailableMaterialLibraries = new ObservableCollection<ExcelProductMaterialLibraryModel>(ExcelDataService.ExcelProductMaterialLibraryModels.FindAll(e => e.ProductName == productName));
                AvailableMaterialLibraries.Add(new ExcelProductMaterialLibraryModel());
            }
        }

        //从模型中选中isChecked的对象
        private void SelectInRevit()
        {
            var elements = ShowElements.GetAllElements().FindAll(a => a.IsChecked);
            SelectElementInRevit(elements);
        }
        //树状图全不选
        private void DeselectAllTreeItems()
        {
            AllElements.SelectAll(false);
            ShowElements.SelectAll(false);
        }
        //树状图全选
        private void SelectAllTreeItems()
        {
            AllElements.SelectAll(false);
            ShowElements.SelectAll(true);
        }

        /// <summary>
        /// 根据element列表直接汇总为materialList
        /// </summary>
        /// <param name="elementIDs"></param>
        /// <returns></returns>
        private void GetMaterialViewModels(List<ElementInstanceViewModel> elementInstances)
        {
            List<RevitSolidElement> revitSolidElementList = new List<RevitSolidElement>();
            foreach (var elementInstance in elementInstances)
            {
                RevitSolidElement revitSolidElement = AllElements.RevitSolidElements.FirstOrDefault(a => a.ID == elementInstance.Id);
                revitSolidElementList.Add(revitSolidElement);
            }
            ShowMaterialList = PropertyMatchTool.FillMaterialList(UiDocument, revitSolidElementList, out List<int> noMatchedElement);
        }

        private void SelectElementInRevit(List<ElementInstanceViewModel> elementInstanceViewModels)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var elementInstance in elementInstanceViewModels)
            {
                elementIds.Add(new ElementId(elementInstance.Id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }

        private void SelectElementInRevit(FamilyCategoryViewModel familyCategory)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var elementInstance in familyCategory.GetAllElementInstanceViewModels())
            {

                elementIds.Add(new ElementId(elementInstance.Id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }

        private void SelectElementInRevit(FamilyViewModel family)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var id in family.IDs)
            {

                elementIds.Add(new ElementId(id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(FamilyExtendViewModel familyExtend)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var id in familyExtend.IDs)
            {
                elementIds.Add(new ElementId(id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(ElementInstanceViewModel elementInstance)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>
            {
                new ElementId(elementInstance.Id)
            };
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void Search()
        {
            if (SearchKeyword == null)
            {
                ResetShowElements();
                return;
            }
            if (_selectedElement == null)
            {
                var revitSolidElements = AllElements.RevitSolidElements.FindAll(a =>
                    a.FamilyName.Contains(SearchKeyword) || a.FamilyCategory.Contains(SearchKeyword) || a.ExtendName.Contains(SearchKeyword));
                ShowElements = new ElementViewModel(revitSolidElements);
            }
            else
            {
                var revitSolidElements = new List<RevitSolidElement>();
                if (_selectedElement.Name == "所有")
                {
                    revitSolidElements = AllElements.RevitSolidElements;
                }
                else
                {
                    revitSolidElements = AllElements.RevitSolidElements.ToList().FindAll(a => a.FamilyCategory == _selectedElement.Name);
                }
                revitSolidElements = revitSolidElements.FindAll(a =>
                    a.FamilyName.Contains(SearchKeyword) || a.FamilyCategory.Contains(SearchKeyword) || a.ExtendName.Contains(SearchKeyword));
                ShowElements = new ElementViewModel(revitSolidElements);
            }
            if (FilterConditions.Count != 0)
            {
                var revitSolidElements = ShowElements.RevitSolidElements.FindAll(a => FilterTool.FilterRevitElement(a, FilterConditions.ToList()));
                ShowElements = new ElementViewModel(revitSolidElements);
            }
            ResetShowElements();
        }

        /// <summary>
        /// 每当改变ShowElements的时候，都需要进行的修改
        /// 在搜索时使用
        /// 在下拉选择族类别时使用
        /// 初始化的时候使用
        /// </summary>
        private void ResetShowElements()
        {
            AllElements.SelectAll(false);
            ShowElements.SelectAll(false);

            EntryParameters();
        }

        private async Task OK()
        {
            try
            {
                var elements = ShowElements.GetAllElements().FindAll(a => a.IsChecked);
                var ids = elements.Select(a => a.Id);
                foreach (var parameter in SelectedItemParameters)
                {
                    if (parameter.IsModified)
                    {
                        await CustomHandler.Run(a =>
                        {
                            SetParameter(a.ActiveUIDocument, parameter, elements);
                        });
                        parameter.IsModified = false;
                    }
                }
                EntryParameters();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void SetParameter(UIDocument uIDocument, ParameterSetVM parameterSet, List<ElementInstanceViewModel> elements)
        {
            var name = parameterSet.Name;
            var value = parameterSet.Value;
            using (Transaction transaction = new Transaction(uIDocument.Document, "SetParameter"))
            {
                transaction.Start();
                if (parameterSet.ValueType == "实例参数")
                {
                    foreach (var elementInstance in elements)
                    {
                        Element element = uIDocument.Document.GetElement(new ElementId(elementInstance.Id));
                        var p = element.LookupParameter(name);
                        if (p == null)
                        {
                            TaskDialog.Show("警告", "当前项目未启用关联数据库参数，无法关联数据库。");
                            transaction.RollBack();
                            return;
                        }
                        if (p.IsReadOnly)
                        {
                            //错误操作需要回滚，重新读取属性表，结束当前录入的操作
                            TaskDialog.Show("错误报告", $"输入参数不可修改，参数：{name} 的值：{value}");
                            transaction.RollBack();
                            EntryParameters();
                            return;
                        }
                        else if (!p.Set(value))
                        {
                            //错误操作需要回滚，重新读取属性表，结束当前录入的操作
                            TaskDialog.Show("错误报告", $"输入参数的值不合法，参数：{name} 的值：{value}");
                            transaction.RollBack();
                            EntryParameters();
                            return;
                        }
                        else
                        {
                            //在修改模型完成后也要将绑定属性的值同步修改
                            var p1 = elementInstance.Parameters.Find(a => a.Name == name);
                            if (p1 != null)
                            {
                                p1.Value = value;
                            }
                        }
                    }
                }
                else if (parameterSet.ValueType == "类型参数")
                {
                    Element element = UiDocument.Document.GetElement(
                        UiDocument.Document.GetElement(new ElementId(elements[0].Id)
                        ).LookupParameter("族与类型")?.AsElementId());
                    var p = element.LookupParameter(name);
                    if (p.IsReadOnly)
                    {
                        //错误操作需要回滚，重新读取属性表，结束当前录入的操作
                        TaskDialog.Show("错误报告", $"输入参数不可修改，参数：{name} 的值：{value}");
                        transaction.RollBack();
                        EntryParameters();
                        return;
                    }
                    else if (!p.Set(value))
                    {
                        //错误操作需要回滚，重新读取属性表，结束当前录入的操作
                        TaskDialog.Show("错误报告", $"输入参数的值不合法，参数：{name} 的值：{value}");
                        transaction.RollBack();
                        EntryParameters();
                        return;
                    }
                    else
                    {
                        //在修改模型完成后也要将绑定属性的值同步修改
                        elements.ForEach(e => e.Parameters.Find(a => a.Name == name).Value = value);
                    }
                }
                transaction.Commit();
            }
        }

        internal List<string> GetFilterPropertyList()
        {
            var elements = ShowElements.GetAllElements();
            var properties = new List<string>();
            elements.ForEach(e => e.Parameters.ForEach(p => properties.Add(p.Name)));
            return properties.Distinct().ToList();
        }

        internal IEnumerable GetFilterValueList()
        {
            var name = SelectedFilterProperty;
            var elements = ShowElements.GetAllElements();
            var result = new List<string>();
            elements.ForEach(e =>
            {
                var parameters = e.Parameters.FindAll(p => p.Name == name);
                parameters?.ForEach(p => result.Add(p.Value));
            });
            result = result.Distinct().ToList();
            if (result.Contains(null))
            {
                result.Remove(null);
                result.Add("");
            }
            return result.Distinct().ToList();
        }
    }
}
