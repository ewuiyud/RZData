using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using RZData.Extensions;
using RZData.Models;
using RZData.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace RZData.Services
{
    public class RevitElementService
    {
        //输出所有的元素数据用于测试
        public void OutputAllElements(UIDocument UiDocument)
        {
            var elementDataList = new List<Dictionary<string, object>>();
            var document = UiDocument.Document;
            var collector = new FilteredElementCollector(document);
            var elements = collector.WhereElementIsNotElementType();
            List<string> categortyFilter = new List<string>
            {
                "图框",
                "图纸",
                "明细表",
                "视图",
                "标高",
                "视口",
                "常规注释",
                "文字注释",
                "立面",
                "详图项目",
                "自动绘制尺寸标注",
                "导线",
                "尺寸标注",
                "导线标记",
                "详图项目标记",
                "相机",
                "多类别标记",
            };
            foreach (var element in elements)
            {
                //var familyElementID = element.LookupParameter("族与类型")?.AsElementId();
                //var familyElement = document.GetElement(familyElementID);
                var familyCategory = element.GetFamilyCategory();
                if (categortyFilter.Contains(familyCategory))
                {
                    continue;
                }
                var familyName = element.GetFamilyName();
                var extendName = element.GetExtendName();
                if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(familyCategory) || string.IsNullOrEmpty(extendName))
                {
                    continue;
                }
                var parameters = element.Parameters.GetEnumerator();
                var pars = new List<string>();
                while (parameters.MoveNext())
                {
                    var parameter = parameters.Current as Parameter;
                    if (parameter.Definition == null)
                    {
                        continue;
                    }
                    if (!pars.Contains((parameter.Definition.Name + ":" + parameter.GetValue())))
                    {
                        pars.Add((parameter.Definition.Name + ":" + parameter.GetValue()));
                    }
                }
                //输出为json格式文件
                var elementData = new Dictionary<string, object>
                {
                    { "FamilyCategory", familyCategory },
                    { "FamilyName", familyName },
                    { "ExtendName", extendName },
                    { "Parameters", pars }
                };

                elementDataList.Add(elementData);
            }
            // 序列化为 JSON 格式
            var json = JsonConvert.SerializeObject(elementDataList, Newtonsoft.Json.Formatting.Indented);

            // 输出为 JSON 格式文件
            var outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "RevitElements.json");
            File.WriteAllText(outputPath, json);

            TaskDialog.Show("输出完成", $"所有元素数据已输出到 {outputPath}");
        }
        public ObservableCollection<RevitSolidElement> LoadAllRevitElements(UIDocument UiDocument)
        {
            List<ExcelFamilyNameModel> records = ExcelDataService.ExcelFamilyRecords;
            //表格中以MIC开头的族为可加载族，其他为系统族
            var systemFamilyDictionary = records.FindAll(a => !a.FamilyName.StartsWith(ConstString.ParameterPrex));
            var loadableFamilyDictionary = records.FindAll(a => a.FamilyName.StartsWith(ConstString.ParameterPrex));
            var familyList = new List<string>();
            records.ForEach(a => { if (!familyList.Contains(a.FamilyCategory)) familyList.Add(a.FamilyCategory); });
            var document = UiDocument.Document;
            var collector = new FilteredElementCollector(document);
            var elements = collector.WhereElementIsNotElementType();

            ObservableCollection<RevitSolidElement> AllSolidElements = new ObservableCollection<RevitSolidElement>();

            foreach (var element in elements)
            {
                //若符合过滤规则，直接跳过
                if (SelectiveFiltering(element))
                {
                    continue;
                }
                if (familyList.Contains(element.GetFamilyCategory()))
                {
                    if (element is FamilyInstance familyInstance)
                    {
                        var revitSolidElement = new RevitSolidElement(element);
                        ProcessFamilyInstance(loadableFamilyDictionary, document, element, revitSolidElement);
                        AllSolidElements.Add(revitSolidElement);
                    }
                    else
                    {
                        var revitSolidElement = new RevitSolidElement(element);
                        ProcessNonFamilyInstance(systemFamilyDictionary, document, element, revitSolidElement);
                        AllSolidElements.Add(revitSolidElement);
                    }
                }
            }
            return AllSolidElements;
        }
        /// <summary>
        /// 过滤掉部分指定的元素
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool SelectiveFiltering(Element element)
        {
            string category = element.GetFamilyCategory();
            string family = element.GetFamilyName();
            string name = element.Name;
            if (category == "幕墙嵌板")
            {
                if (family == "系统嵌板")
                {
                    if (name == "墙" || name == "玻璃")
                        return true;
                }
            }
            Dictionary<string, List<string>> filterList = new Dictionary<string, List<string>>
            {
                {"墙",new List<string>{"幕墙" } },
                {"幕墙嵌板",new List<string>{"空系统嵌板"} }
            };
            if (category != null && filterList.Keys.Contains(category))
            {
                return filterList[category].Contains(family);
            }

            return false;
        }
        /// <summary>
        /// 系统族检查
        /// </summary>
        /// <param name="systemFamilyDictionary"></param> 
        /// <param name="document"></param>
        /// <param name="element"></param>
        /// <param name="revitSolidElement"></param>
        public void ProcessNonFamilyInstance(List<ExcelFamilyNameModel> systemFamilyDictionary,
            Document document, Element element, RevitSolidElement revitSolidElement)
        {
            var extendName = element.GetExtendName();
            var typeNames = systemFamilyDictionary.FindAll(a => CheckRecordExtendName(a, document, element)).ToList();
            if (typeNames.Count() == 0 || !typeNames.Exists(a => a.FamilyName == element.GetFamilyName() && a.FamilyCategory == element.GetFamilyCategory()))
            {
                revitSolidElement.IsNameCorrect = false;
            }
            else
            {
                revitSolidElement.IsNameCorrect = true;
                var record = typeNames.First(a => a.FamilyName == element.GetFamilyName());
                revitSolidElement.ElementName = record.ElementName;
                CheckParameters(record, document, element, revitSolidElement);
            }
        }
        /// <summary>
        /// 载入族检查
        /// </summary>
        /// <param name="loadableFamilyDictionary"></param>
        /// <param name="document"></param>
        /// <param name="element"></param>
        /// <param name="revitSolidElement"></param>
        public void ProcessFamilyInstance(List<ExcelFamilyNameModel> loadableFamilyDictionary,
            Document document, Element element, RevitSolidElement revitSolidElement)
        {
            var typeName = element.GetFamilyName();
            var typeNames = loadableFamilyDictionary.FindAll(a => typeName.StartsWith(a.FamilyName.Substring(0, a.FamilyName.Length - 1))).ToList();
            typeNames = typeNames.FindAll(a => CheckRecordExtendRequired(a, document, element)).ToList();
            if (typeNames.Count() == 0 || !typeNames.Exists(a => element.GetFamilyCategory() == a.FamilyCategory))
            {
                revitSolidElement.IsNameCorrect = false;
            }
            else
            {
                revitSolidElement.IsNameCorrect = true;
                var record = typeNames.First(a => a.FamilyCategory == element.GetFamilyCategory());
                revitSolidElement.ElementName = record.ElementName;
                CheckParameters(record, document, element, revitSolidElement);
            }
        }
        public bool CheckRecordExtendName(ExcelFamilyNameModel excelRecord, Document document, Element element)
        {
            var recordExtendName = excelRecord.ExtendName;
            string incorrectMessage = $"补充属性不合理，族：{excelRecord.FamilyCategory} 类型：{excelRecord.FamilyName} 补充属性：{excelRecord.ExtendName}";
            if (recordExtendName.Contains("&&"))
            {
                var requires = recordExtendName.Split(new[] { "&&" }, StringSplitOptions.None);
                //存在多个条件时，
                return requires.All(a =>
                {
                    if (a.StartsWith(ConstString.ExtendNamePrefix))
                    {
                        return element.GetExtendName().IsSameAs(a.Replace(ConstString.ExtendNamePrefix, ""));
                    }
                    else
                    {
                        var str = a.Split('=');
                        if (str.Count() != 2)
                        {
                            TaskDialog.Show("错误信息", incorrectMessage);
                            return false;
                        }
                        var value = element.GetElementValue(document, str[0]);
                        if (value == null)
                        {
                            return false;
                        }
                        return value.IsSameAs(str[1]);
                    }
                });
            }
            else
            {
                if (recordExtendName.StartsWith(ConstString.ExtendNamePrefix))
                {
                    return element.GetExtendName().StartsWith(recordExtendName.Substring(3, recordExtendName.Length - 4));
                }
                else
                {
                    //TaskDialog.Show("错误信息", incorrectMessage); 
                    return false;
                }
            }
        }
        /// <summary>
        /// 如果在载入族中的补充属性中，有附加条件，那么需要判断附加条件是否满足
        /// </summary>
        /// <param name="excelRecord"></param>
        /// <param name="document"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool CheckRecordExtendRequired(ExcelFamilyNameModel excelRecord, Document document, Element element)
        {
            //若为不填，则不需要考虑
            if (excelRecord.ExtendName == "不填")
            {
                return true;
            }
            string incorrectMessage = $"补充属性不合理，族：{excelRecord.FamilyCategory} 类型：{excelRecord.FamilyName} 补充属性：{excelRecord.ExtendName}";
            var requires = excelRecord.ExtendName.Split(new[] { "&&" }, StringSplitOptions.None);
            //存在多个条件时，
            return requires.All(a =>
            {
                var require = a.Split('=');
                if (require.Count() != 2)
                {
                    TaskDialog.Show("错误信息", incorrectMessage);
                    return false;
                }
                var value = element.GetElementValue(document, require[0]);
                if (value == null)
                {
                    return false;
                }
                return value.IsSameAs(require[1]);
            });
        }
        public bool CheckParameters(ExcelFamilyNameModel excelRecord, Document document, Element element, RevitSolidElement revitSolidElement)
        {
            var familyElementID = element.LookupParameter("族与类型")?.AsElementId();
            var familyElement = document.GetElement(familyElementID);

            foreach (var excelParameter in excelRecord.RequiredProperties)
            {
                ParameterVM parameterVM = new ParameterVM();
                parameterVM.Name = excelParameter.Name;
                parameterVM.Unit = excelParameter.Unit;
                parameterVM.Reference = excelParameter.Reference;
                parameterVM.IsShowed = excelParameter.IsShowed;
                parameterVM.TDCName = excelParameter.TDCName;
                if (!string.IsNullOrEmpty(excelParameter.StandardValue))
                {
                    parameterVM.Value = excelParameter.StandardValue;
                    parameterVM.IsReadOnly = true;
                }
                else
                {
                    var parameter = element.LookupParameter(parameterVM.Name) ?? familyElement?.LookupParameter(parameterVM.Name);
                    if (parameter == null)
                    {
                        parameterVM.Value = ConstString.LossParameterName;
                        parameterVM.IsReadOnly = true;
                    }
                    else
                    {
                        var value = parameter.GetValue();
                        //当有值且存在验证公式的时候
                        if (!string.IsNullOrEmpty(excelParameter.ValueEnumString))
                        {
                            if (value!=null&& Regex.IsMatch(value, excelParameter.ValueEnumString.Trim('/')))
                            {
                                parameterVM.Value = value;
                            }
                            parameterVM.ValueEnum = ExtractRegexOptions(excelParameter.ValueEnumString);
                        }
                        else
                            parameterVM.Value = value;
                    }
                    parameterVM.ValueType = parameter != null ? (parameter.Element.Id == element.Id ? ConstString.InstanceParameterName : ConstString.TypeParameterName) : "";
                }
                revitSolidElement.Parameters.Add(parameterVM);
            }
            revitSolidElement.IsPropertiesCorrect = revitSolidElement.Parameters.All(p => p.Value != ConstString.LossParameterName);
            return revitSolidElement.IsPropertiesCorrect;
        }
        public static List<string> ExtractRegexOptions(string regexPattern)
        {
            List<string> options = new List<string>();

            // 使用竖线分割模式字符串
            string[] parts = regexPattern.Trim('/').Split('|');

            foreach (string part in parts)
            {
                // 去除可能存在的转义字符
                var cleanedPart = part.TrimStart('^');
                cleanedPart = cleanedPart.TrimEnd('$');
                options.Add(cleanedPart);
            }

            return options;
        }
    }
}
