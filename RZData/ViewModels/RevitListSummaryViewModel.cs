using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.Input;
using RZData.Models;
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
        private RevitListSummaryView revitListSummaryView;
        private ObservableCollection<MaterialRecord> _allMaterialList;
        private ObservableCollection<MaterialRecord> _showMaterialList;
        private MaterialRecord _selectedMaterialRecord;
        private ObservableCollection<AssemblyRecord> _showAssemblyList;
        private AssemblyRecord _selectedAssemblyRecord;
        private ObservableCollection<string> _propertyNames;
        private ObservableCollection<string> _propertyValues;
        private string _selectedPropertyName;
        private string _selectedPropertyValue;
        private ObservableCollection<(string, string)> _requiredProperties;


        public ObservableCollection<MaterialRecord> AllMaterialList
        {
            get => _allMaterialList;
            set => SetProperty(ref _allMaterialList, value);
        }
        public ObservableCollection<MaterialRecord> ShowMaterialList
        {
            get => _showMaterialList;
            set => SetProperty(ref _showMaterialList, value);
        }
        public MaterialRecord SelectedMaterialRecord
        {
            get => _selectedMaterialRecord;
            set => SetProperty(ref _selectedMaterialRecord, value);
        }
        public AssemblyRecord SelectedAssemblyRecord
        {
            get => _selectedAssemblyRecord;
            set => SetProperty(ref _selectedAssemblyRecord, value);
        }
        public ObservableCollection<AssemblyRecord> ShowAssemblyList
        {
            get => _showAssemblyList;
            set => SetProperty(ref _showAssemblyList, value);
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
        public RevitListSummaryViewModel(UIDocument uiDocument, DataElement allElements)
        {
            UiDocument = uiDocument;
            AllElements = allElements;
            AllMaterialList = new ObservableCollection<MaterialRecord>();
            ShowMaterialList = new ObservableCollection<MaterialRecord>();
            ShowAssemblyList = new ObservableCollection<AssemblyRecord>();
            PropertyNames = new ObservableCollection<string>();
            PropertyValues = new ObservableCollection<string>();
            RequiredProperties = new ObservableCollection<(string, string)>();
            AddRequiredPropertiesCommand = new RelayCommand(AddRequiredProperties);
            DeleteRequiredPropertiesCommand = new RelayCommand(DeleteRequiredProperties);
            OKWitheRequiredPropertiesCommand = new RelayCommand(OKWitheRequiredProperties);
            DeleteRequiredPropertyCommand = new RelayCommand<(string, string)>(DeleteRequiredProperty);
            CansoleCommand = new RelayCommand(Cansole);
        }

        public void SetView(RevitListSummaryView revitListSummaryView)
        {
            this.revitListSummaryView = revitListSummaryView;
        }
        private void Cansole()
        {
            revitListSummaryView.Close();
        }

        private void OKWitheRequiredProperties()
        {
            ObservableCollection<MaterialRecord> temp = new ObservableCollection<MaterialRecord>();
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

        private bool MatchRequired((string, string) required, MaterialRecord materialRecord)
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
            RequiredProperties.Clear();
        }

        private void AddRequiredProperties()
        {
            if (string.IsNullOrEmpty(SelectedPropertyName))
            {
                return;
            }
            RequiredProperties.Add((SelectedPropertyName, SelectedPropertyValue));
        }

        public void GetMaterialListFromDataElement()
        {
            try
            {
                List<DataInstance> list = GetDataInstanceList(AllElements);
                AllMaterialList = FillMaterialList(list);
                ShowMaterialList = AllMaterialList;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private ObservableCollection<MaterialRecord> FillMaterialList(List<DataInstance> list)
        {
            ObservableCollection<MaterialRecord> result = new ObservableCollection<MaterialRecord>();
            foreach (var dataInstance in list)
            {
                var excelMaterialBusinessRecord = SortMaterials(dataInstance);
                if (excelMaterialBusinessRecord != null)
                {
                    var materialRecord = new MaterialRecord();
                    materialRecord.MaterialName = excelMaterialBusinessRecord.Name;
                    if (excelMaterialBusinessRecord.UsageLocation.Count() > 5)
                    {
                        var input = excelMaterialBusinessRecord.UsageLocation.Split('：')[1];
                        materialRecord.UsageMethod = ExplainString(input, dataInstance) + "使用";
                    }
                    materialRecord.ProjectFeaturesDetail = ExplainProjectFeatures(
                        excelMaterialBusinessRecord.ProjectCharacteristics, dataInstance);
                    var m = result.FirstOrDefault(
                        a => a.MaterialName == materialRecord.MaterialName
                        && a.UsageMethod == materialRecord.UsageMethod
                        && a.ProjectFeatures == materialRecord.ProjectFeatures);
                    if (m != null)
                    {
                        m.DataInstances.Add(dataInstance);
                    }
                    else
                    {
                        materialRecord.DataInstances.Add(dataInstance);
                        result.Add(materialRecord);
                    }
                }
            }
            return result;
        }

        private List<DataInstance> GetDataInstanceList(DataElement dataElement)
        {
            List<DataInstance> list = new List<DataInstance>();
            foreach (var family in dataElement.Families)
            {
                foreach (var type in family.FamilyTypes)
                {
                    foreach (var familyExtend in type.FamilyExtends)
                    {
                        if (familyExtend.IsNameCorrect)
                            foreach (var dataInstance in familyExtend.DataInstances)
                            {
                                if (dataInstance.IsPropertiesCorrect)
                                    list.Add(dataInstance);
                            }
                    }
                }
            }
            return list;
        }
        enum MatchedType
        {
            元素分类名称,
            产品分类名称,
            空间分类名称
        }
        private ExcelMaterialBusinessRecord SortMaterials(DataInstance dataInstance)
        {
            foreach (var excelMaterialBusinessRecord in ExcelDataHelper.ExcelMaterialBusinessRules)
            {
                //如果三项都为空，则认为是父级分类
                if (string.IsNullOrEmpty(excelMaterialBusinessRecord.ElementName)
                    && string.IsNullOrEmpty(excelMaterialBusinessRecord.ProductName)
                    && string.IsNullOrEmpty(excelMaterialBusinessRecord.SpaceName))
                    continue;
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ElementName))
                {
                    //根据元素分类名称匹配
                    var elementName = dataInstance.ElementName;
                    if (elementName == null)
                        continue;
                    if (!IsStringMatchRule(elementName, excelMaterialBusinessRecord.ElementName, MatchedType.元素分类名称))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ProductName))
                {
                    var p = dataInstance.Parameters.FirstOrDefault(a => a.TDCName == "TDC-产品分类名称");
                    if (p == null)
                        continue;
                    if (!IsStringMatchRule(p.Value, excelMaterialBusinessRecord.ProductName, MatchedType.产品分类名称))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.SpaceName))
                {
                    var p = dataInstance.Parameters.FirstOrDefault(a => a.TDCName == "TDC-空间分类名称");
                    if (p == null)
                        continue;
                    if (!IsStringMatchRule(p.Value, excelMaterialBusinessRecord.SpaceName, MatchedType.空间分类名称))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ExtendRule))
                {
                    if (!IsDatainstanceMatchExtendRule(dataInstance, excelMaterialBusinessRecord.ExtendRule))
                        continue;
                }
                return excelMaterialBusinessRecord;
            }
            return null;
        }
        (string, string) ExplainCodeProperty(string input, DataInstance dataInstance)
        {
            var dictionary = ExcelDataHelper.ExcelPropertyDic;
            string temp = input;
            string prefix = temp.Split('：')[0];
            string suffix = temp.Split('：')[1];
            if (!suffix.Contains("《"))
            {
                return (prefix.Substring(2), suffix);
            }
            return (prefix.Substring(2), ExplainString(suffix, dataInstance));
        }
        string ExplainString(string input, DataInstance dataInstance)
        {
            var dictionary = ExcelDataHelper.ExcelPropertyDic;
            if (!input.Contains("《"))
            {
                return input;
            }
            int startIndex = input.IndexOf("《");
            int endIndex = input.IndexOf("》");
            string key = input.Substring(startIndex + 1, endIndex - startIndex - 1);
            if (dictionary.Keys.Contains(key))
            {
                var tDCName = dictionary[key];
                if (tDCName == "TDC-元素分类名称")
                {
                    return dataInstance.ElementName;
                }
                else
                {
                    var p = dataInstance.Parameters.FirstOrDefault(a => a.TDCName == tDCName);
                    if (p != null)
                    {
                        return p.Value;
                    }
                    else
                    {
                        return "未识别属性，请检查模板对应词条";
                    }
                }
            }
            else
            {
                TaskDialog.Show("错误信息", $"需要匹配的项目特征：{input}， 不合法");
                throw new Exception($"需要匹配的项目特征：{input}， 不合法。");
            }
        }
        Dictionary<string, string> ExplainProjectFeatures(string input, DataInstance dataInstance)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            var features = input.Split('\n');
            var result = new Dictionary<string, string>();
            foreach (var feature in features)
            {
                var temp = ExplainCodeProperty(feature, dataInstance);
                result.Add(temp.Item1, temp.Item2);
            }
            return result;
        }
        private bool IsStringMatchRule(string input, string rule, MatchedType matchedType)
        {
            if (!rule.StartsWith("{{") || !rule.EndsWith("}}"))
            {
                return input == rule;
            }
            string ruleContent = rule.Substring(2, rule.Length - 4);
            string[] conditions = ruleContent.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string condition in conditions)
            {
                if (condition.StartsWith("$="))
                {
                    string value = condition.Substring(2);
                    switch (matchedType)
                    {
                        case MatchedType.元素分类名称:
                            var elementNode = ExcelDataHelper.ExcelElementCode.FirstOrDefault(a => a.Value == value);
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
                            var productNode = ExcelDataHelper.ExcelProductCode.FirstOrDefault(a => a.Value == value);
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

                }
            }
            return false;
        }
        private bool IsDatainstanceMatchExtendRule(DataInstance dataInstance, string rule)
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
                var p = dataInstance.Parameters.FirstOrDefault(a => a.TDCName == propertyName);
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
            ShowAssemblyList = new ObservableCollection<AssemblyRecord>();
            if (SelectedMaterialRecord != null)
                foreach (var dataInstance in SelectedMaterialRecord.DataInstances)
                {
                    ShowAssemblyList.Add(new AssemblyRecord()
                    {
                        AssemblyID = dataInstance.Element.Id.ToString(),
                        AssemblyName = dataInstance.Element.LookupParameter("族与类型").AsValueString(),
                        Modelbelonging = dataInstance.Element.Document == UiDocument.Document ? "当前模型" : "链接模型"
                    });
                }
        }

        internal void PropertyNameDroped()
        {
            PropertyNames = new ObservableCollection<string>();
            foreach (var materialRecord in ShowMaterialList)
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

        internal void PropertyValueDroped()
        {
            PropertyValues = new ObservableCollection<string>();
            if (string.IsNullOrEmpty(SelectedPropertyName))
            {
                return;
            }
            switch (SelectedPropertyName)
            {
                case "材料名称":
                    foreach (var materialRecord in ShowMaterialList)
                    {
                        if (!PropertyValues.Contains(materialRecord.MaterialName))
                        {
                            PropertyValues.Add(materialRecord.MaterialName);
                        }
                    }
                    break;
                case "使用方式":
                    foreach (var materialRecord in ShowMaterialList)
                    {
                        if (!PropertyValues.Contains(materialRecord.UsageMethod))
                        {
                            PropertyValues.Add(materialRecord.UsageMethod);
                        }
                    }
                    break;
                default:
                    foreach (var materialRecord in ShowMaterialList)
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
        private void DeleteRequiredProperty((string, string) parameter)
        {
            if (RequiredProperties.Contains(parameter))
                RequiredProperties.Remove(parameter);
        }
        internal void DoubleClickAndPickObjects()
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            elementIds.Add(new ElementId(int.Parse(SelectedAssemblyRecord.AssemblyID)));
            uidoc.Selection.SetElementIds(elementIds);
        }
    }
}
