using Autodesk.Revit.UI;
using RZData.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RZData.ViewModels
{
    public class RevitListSummaryViewModel : BaseViewModel
    {
        private ObservableCollection<MaterialRecord> _allMaterialList;
        private ObservableCollection<MaterialRecord> _showMaterialList;
        private MaterialRecord _selectedMaterialRecord;
        private ObservableCollection<AssemblyRecord> _showAssemblyList;
        private ObservableCollection<string> _propertyNames;
        private ObservableCollection<string> _propertyValues;
        private string _selectedPropertyName;
        private string _selectedPropertyValue;
        private ObservableCollection<object> _requiredProperties;


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
        public ObservableCollection<object> RequiredProperties
        {
            get => _requiredProperties;
            set => SetProperty(ref _requiredProperties, value);
        }

        public RevitListSummaryViewModel(UIDocument uiDocument, DataElement allElements)
        {
            UiDocument = uiDocument;
            AllElements = allElements;
            AllMaterialList = new ObservableCollection<MaterialRecord>();
            ShowMaterialList = new ObservableCollection<MaterialRecord>();
            ShowAssemblyList = new ObservableCollection<AssemblyRecord>();
            PropertyNames = new ObservableCollection<string>();
            PropertyValues = new ObservableCollection<string>();
            RequiredProperties = new ObservableCollection<object>();
        }

        public void GetMaterialListFromDataElement()
        {
            AllMaterialList = new ObservableCollection<MaterialRecord>();
            List<DataInstance> list = new List<DataInstance>();
            foreach (var family in AllElements.Families)
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

            foreach (var dataInstance in list)
            {
                var excelMaterialBusinessRecord = SortMaterials(dataInstance);
                if (excelMaterialBusinessRecord != null)
                {
                    var materialRecord = new MaterialRecord();
                    materialRecord.MaterialName = excelMaterialBusinessRecord.Name;
                    materialRecord.UsageMethod = excelMaterialBusinessRecord.UsageLocation.Count() > 5 ?
                        ExplainCodeProperty(excelMaterialBusinessRecord.UsageLocation, dataInstance).Substring(5) + "使用" : "";
                    materialRecord.ProjectFeatures = ExplainProjectFeatures(
                        excelMaterialBusinessRecord.ProjectCharacteristics, dataInstance);
                    var m = AllMaterialList.FirstOrDefault(
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
                        AllMaterialList.Add(materialRecord);
                    }
                }
            }
            ShowMaterialList = AllMaterialList;
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
                    var elementName = dataInstance.ElementName;
                    if (elementName == null)
                        continue;
                    if (!IsStringMatchRule(elementName, excelMaterialBusinessRecord.ElementName))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ProductName))
                {
                    var p = dataInstance.Parameters.FirstOrDefault(a => a.TDCName == "TDC-产品分类名称");
                    if (p == null)
                        continue;
                    if (!IsStringMatchRule(p.Value, excelMaterialBusinessRecord.ProductName))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.SpaceName))
                {
                    var p = dataInstance.Parameters.FirstOrDefault(a => a.TDCName == "TDC-空间分类名称");
                    if (p == null)
                        continue;
                    if (!IsStringMatchRule(p.Value, excelMaterialBusinessRecord.SpaceName))
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
        string ExplainCodeProperty(string input, DataInstance dataInstance)
        {
            var dictionary = ExcelDataHelper.ExcelPropertyDic;
            string temp = input;
            string prefix = temp.Split('：')[0];
            string suffix = temp.Split('：')[1];
            if (!suffix.Contains("《"))
            {
                return input;
            }
            int startIndex = suffix.IndexOf("《");
            int endIndex = suffix.IndexOf("》");
            string key = suffix.Substring(startIndex + 1, endIndex - startIndex - 1);
            if (dictionary.Keys.Contains(key))
            {
                var tDCName = dictionary[key];
                if (tDCName == "TDC-元素分类名称")
                {
                    temp = prefix + "：" + dataInstance.ElementName;
                }
                else
                {
                    var p = dataInstance.Parameters.FirstOrDefault(a => a.TDCName == tDCName);
                    if (p != null)
                    {
                        temp = prefix + "：" + p.Value;
                    }
                    else
                    {
                        temp = prefix + "：未识别属性，请检查模板对应词条";
                    }
                }
                return temp;
            }
            else
            {
                TaskDialog.Show("错误信息", $"需要匹配的项目特征：{input}， 不合法");
                throw new Exception($"需要匹配的项目特征：{input}， 不合法。");
            }
        }
        string ExplainProjectFeatures(string input, DataInstance dataInstance)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }
            var features = input.Split('\n');
            string result = string.Empty;
            foreach (var feature in features)
            {
                string temp = ExplainCodeProperty(feature, dataInstance);
                result += temp + "\n";
            }
            return result;
        }
        private bool IsStringMatchRule(string input, string rule)
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
                    if (input == value)
                    {
                        return true;
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
            if (SelectedMaterialRecord!=null)
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
    }
}
