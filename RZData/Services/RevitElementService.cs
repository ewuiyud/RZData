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
            List<ExcelFamilyRecord> records = ExcelDataService.ExcelFamilyRecords;
            //表格中以MIC开头的族为可加载族，其他为系统族
            var systemFamilyDictionary = records.FindAll(a => !a.FamilyName.StartsWith("MIC"));
            var loadableFamilyDictionary = records.FindAll(a => a.FamilyName.StartsWith("MIC"));
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
                        var revitSolidElement = new RevitSolidElement(element, RevitElementFamilyType.LoadFamilyElement);
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
        public void ProcessNonFamilyInstance(List<ExcelFamilyRecord> systemFamilyDictionary,
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
        public void ProcessFamilyInstance(List<ExcelFamilyRecord> loadableFamilyDictionary,
            Document document, Element element, RevitSolidElement revitSolidElement)
        {
            var typeName = element.GetFamilyName();
            var typeNames = loadableFamilyDictionary.FindAll(a => typeName.StartsWith(a.FamilyName.Substring(0, a.FamilyName.Length - 1))).ToList();
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
        public bool CheckRecordExtendName(ExcelFamilyRecord excelRecord, Document document, Element element)
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
                            TaskDialog.Show("错误信息", incorrectMessage);
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
        public bool CheckParameters(ExcelFamilyRecord excelRecord, Document document, Element element, RevitSolidElement revitSolidElement)
        {
            var familyElementID = element.LookupParameter("族与类型")?.AsElementId();
            var familyElement = document.GetElement(familyElementID);

            foreach (var propertyName in excelRecord.RequiredProperties)
            {
                var parameter = element.LookupParameter(propertyName.Value) ?? familyElement?.LookupParameter(propertyName.Value);
                var name = propertyName.Value;
                var value = parameter != null ? parameter.GetValue() : "缺失";
                var tdcName = propertyName.Key;
                var type = parameter != null ? (parameter.Element.Id == element.Id ? "实例参数" : "类型参数") : "";
                revitSolidElement.Parameters.Add(new ParameterVM(name, value, tdcName, type));
            }
            revitSolidElement.IsPropertiesCorrect = revitSolidElement.Parameters.All(p => p.Value != "缺失");
            return revitSolidElement.IsPropertiesCorrect;
        }
    }
}
