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
                AllMaterialList = FillMaterialList(list);
                ShowMaterialList = AllMaterialList;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private ObservableCollection<MaterialViewModel> FillMaterialList(List<RevitSolidElement> list)
        {
            ObservableCollection<MaterialViewModel> result = new ObservableCollection<MaterialViewModel>();
            foreach (var revitSolidElement in list)
            {
                var record = SortMaterials(revitSolidElement);
                if (record != null)
                {
                    var materialRecord = new MaterialViewModel
                    {
                        MaterialName = record.Name
                    };
                    if (!string.IsNullOrEmpty(record.ID))
                    {
                        materialRecord.ID = record.ID;
                    }
                    if (!string.IsNullOrEmpty(record.UsageLocation))
                    {
                        materialRecord.UsageMethod = ExplainString(record.UsageLocation, revitSolidElement);
                    }
                    materialRecord.ProjectFeatures = ExplainString(record.ProjectCharacteristics, revitSolidElement);
                    materialRecord.ProjectFeaturesDetail = ExplainProjectFeatures(materialRecord.ProjectFeatures);
                    if (!string.IsNullOrEmpty(record.Unit))
                    {
                        materialRecord.ModelEngineeringUnit = record.Unit;
                    }
                    if (!string.IsNullOrEmpty(record.Quantity))
                    {
                        var mqStr = ExplainString(record.Quantity, revitSolidElement);
                        try
                        {
                            // 使用DataTable的Compute方法计算表达式
                            double mq = Convert.ToDouble(new System.Data.DataTable().Compute(mqStr, null));
                            materialRecord.ModelEngineeringQuantity += mq;
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("警告", $"表达式计算失败: {mqStr}\t\n错误详情:{ex}");
                        }
                    }
                    var m = result.FirstOrDefault(
                        a => a.MaterialName == materialRecord.MaterialName
                        && a.UsageMethod == materialRecord.UsageMethod
                        && a.ProjectFeatures == materialRecord.ProjectFeatures);
                    if (m != null)
                    {
                        m.RevitSolidElements.Add(revitSolidElement);
                        m.ModelEngineeringQuantity += materialRecord.ModelEngineeringQuantity;
                    }
                    else
                    {
                        materialRecord.RevitSolidElements.Add(revitSolidElement);
                        result.Add(materialRecord);
                    }
                }
                else
                {
                    var element = UiDocument.Document.GetElement(new ElementId(revitSolidElement.ID));
                    UnmatchedAssemblyList.Add(new AssemblyViewModel()
                    {
                        AssemblyID = element.Id.ToString(),
                        AssemblyName = element.LookupParameter("族与类型").AsValueString(),
                        //Modelbelonging = element.Document == UiDocument.Document ? "当前模型" : "链接模型"
                    });
                }
            }
            return result;
        }
        enum MatchedType
        {
            元素分类名称,
            产品分类名称,
            空间分类名称
        }
        private ExcelMaterialBusinessRecord SortMaterials(RevitSolidElement revitSolidElement)
        {
            foreach (var excelMaterialBusinessRecord in ExcelDataService.ExcelMaterialBusinessRules)
            {
                //如果三项都为空，则认为是父级分类
                if (string.IsNullOrEmpty(excelMaterialBusinessRecord.ElementName)
                    && string.IsNullOrEmpty(excelMaterialBusinessRecord.ProductName)
                    && string.IsNullOrEmpty(excelMaterialBusinessRecord.SpaceName))
                    continue;
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ElementName))
                {
                    //根据元素分类名称匹配
                    var elementName = revitSolidElement.ElementName;
                    if (elementName == null)
                        continue;
                    if (!IsStringMatchRule(elementName, excelMaterialBusinessRecord.ElementName, MatchedType.元素分类名称))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ProductName))
                {
                    var p = revitSolidElement.Parameters.FirstOrDefault(a => a.TDCName == "TDC-产品分类名称");
                    if (p == null)
                        continue;
                    if (!IsStringMatchRule(p.Value, excelMaterialBusinessRecord.ProductName, MatchedType.产品分类名称))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.SpaceName))
                {
                    var p = revitSolidElement.Parameters.FirstOrDefault(a => a.TDCName == "TDC-空间分类名称");
                    if (p == null)
                        continue;
                    if (!IsStringMatchRule(p.Value, excelMaterialBusinessRecord.SpaceName, MatchedType.空间分类名称))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ExtendRule))
                {
                    if (!IsDatainstanceMatchExtendRule(revitSolidElement, excelMaterialBusinessRecord.ExtendRule))
                        continue;
                }
                return excelMaterialBusinessRecord;
            }
            return null;
        }
        (string, string) ExplainCodeProperty(string input)
        {
            string temp = input;
            if (!temp.Contains('：'))
            {
                return (null,null);
            }
            string prefix = temp.Split('：')[0];
            string suffix = temp.Split('：')[1];
            return (prefix.Substring(2), suffix);
        }
        // 转换方法
        string GetModelEngineeringQuantityValue(string valueName, RevitSolidElement revitSolidElement)
        {
            if (valueName == "TDC-元素分类名称")
            {
                return revitSolidElement.ElementName;
            }

            if (valueName == "数量")
            {
                return "1";
            }

            var doc = UiDocument.Document;
            Element element = doc.GetElement(new ElementId(revitSolidElement.ID));
            var result = element.GetElementValue(doc, valueName);
            if (result != null)
            {
                return result;
            }

            return "0";
        }
        /// <summary>
        /// 新的解释字符串的方法，旧的方法应该在下次更新中删除
        ///1. 字符串中《xx》包裹的内容为查询值，可以从定义好的dictionary中查询对应的值；
        ///2. 字符传中((xx))包裹的内容为固定字符串，直接保留；
        ///3. 字符串[[xx]]包裹的内容为Revit中的元素参数；
        ///4. 字符串中%%xx%%包裹的内容为注释，直接忽略掉。
        /// </summary>
        /// <param name="input"></param>
        /// <param name="revitSolidElement"></param>
        /// <returns></returns>
        internal string ExplainString(string input, RevitSolidElement revitSolidElement)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string result = input;
            var dictionary = ExcelDataService.ExcelPropertyDic;
            // 1. 处理《xx》格式 - 字典查询
            result = Regex.Replace(result, @"《([^》]+)》", match =>
            {
                string key = match.Groups[1].Value;
                if (!dictionary.ContainsKey(key))
                {
                    return $"未在属性列表中找到对应的属性id{input}";
                }
                var tDCName = dictionary[key];
                if (tDCName == "TDC-元素分类名称")
                {
                    return revitSolidElement.ElementName;
                }
                else
                {
                    var p = revitSolidElement.Parameters.FirstOrDefault(a => a.TDCName == tDCName);
                    if (p != null)
                    {
                        return p.Value;
                    }
                    else
                    {
                        return $"不存在的属性项：{tDCName}";
                    }
                }
            });

            // 2. 处理((xx))格式 - 固定字符串，去掉括号
            result = Regex.Replace(result, @"\(\(([^)]+)\)\)", match =>
            {
                return match.Groups[1].Value;
            });

            //3.处理[[xx]]格式 - Revit中的元素参数；
            result = Regex.Replace(result, @"\[\[([^)]+)\]\]", match =>
            {
                return GetModelEngineeringQuantityValue(match.Groups[1].Value, revitSolidElement);
            });

            // 4. 处理%%xx%%格式 - 注释，直接移除
            result = Regex.Replace(result, @"%%[^%]*%%", "");

            return result;
        }
        Dictionary<string, string> ExplainProjectFeatures(string input)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(input))
            {
                return result;
            }
            var features = input.Split('\n');
            foreach (var feature in features)
            {
                var temp = ExplainCodeProperty(feature);
                //对于不包含‘：’的字符串，返回结果为（null,null），不做处理
                if (temp.Item1==null)
                {
                    continue;
                }
                result.Add(temp.Item1, temp.Item2);
            }
            return result;
        }
        private bool IsStringMatchRule(string input, string rule, MatchedType matchedType)
        {
            //if (!rule.StartsWith("{{") || !rule.EndsWith("}}"))
            //{
            //    return input == rule;
            //}
            //string ruleContent = rule.Substring(2, rule.Length - 4);
            string[] conditions = rule.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string value in conditions)
            {
                //if (condition.StartsWith("$="))
                //{
                //    string value = condition.Substring(2);
                switch (matchedType)
                {
                    case MatchedType.元素分类名称:
                        var elementNode = ExcelDataService.ExcelElementCode.FirstOrDefault(a => a.Value == value);
                        if (elementNode == null || elementNode.Children.Count == 0)
                        {
                            if (input == value)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            foreach (var item in elementNode.GetAllChirlds())
                            {
                                if (input == item.Value)
                                {
                                    return true;
                                }
                            }
                        }
                        break;
                    case MatchedType.产品分类名称:
                        var productNode = ExcelDataService.ExcelProductCode.FirstOrDefault(a => a.Value == value);
                        if (productNode == null || productNode.Children.Count == 0)
                        {
                            if (input == value)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            foreach (var item in productNode.GetAllChirlds())
                            {
                                if (input == item.Value)
                                {
                                    return true;
                                }
                            }
                        }
                        break;
                    case MatchedType.空间分类名称:
                        throw new NotSupportedException("空间分类表未完成");
                    default:
                        break;
                }

                //}
            }
            return false;
        }
        private bool IsDatainstanceMatchExtendRule(RevitSolidElement revitSolidElement, string rule)
        {
            string[] parts = rule.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                int operatorIndex = trimmedPart.IndexOfAny(new[] { '=', '!' });

                if (operatorIndex < 0)
                {
                    continue;
                }

                string propertyName = trimmedPart.Substring(0, operatorIndex).Trim();
                string propertyValue = trimmedPart.Substring(operatorIndex + 2).Trim();
                bool isEqual = trimmedPart[operatorIndex] == '=';
                var p = revitSolidElement.Parameters.FirstOrDefault(a => a.TDCName == propertyName);
                if (p != null)
                {
                    if (isEqual)
                    {
                        if (IsMatch(p.Value, propertyValue))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (!IsMatch(p.Value, propertyValue))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        public bool IsMatch(string input, string wildcardPattern)
        {
            if (!wildcardPattern.Contains("*") && !wildcardPattern.Contains("?"))
            {
                return string.Equals(input, wildcardPattern);
            }
            string regexPattern = Regex.Escape(wildcardPattern)
             .Replace(@"\*", ".*");
            regexPattern = $"^{regexPattern}$";
            return Regex.IsMatch(input, regexPattern);
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
