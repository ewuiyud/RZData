using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Extensions;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Parameter = RZData.Models.Parameter;

namespace RZData.Services
{
    public class RevitElementService
    {
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
            DataElement revitElementData = new DataElement();

            foreach (var element in elements)
            {
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
        private void ProcessNonFamilyInstance(List<ExcelFamilyRecord> systemFamilyDictionary,
            Document document, Element element, RevitSolidElement revitSolidElement)
        {
            var extendName = element.GetExtendName();
            var typeNames = systemFamilyDictionary.FindAll(a => CheckRecordExtendName(a, element)).ToList();
            if (typeNames.Count() == 0 || !typeNames.Exists(a => a.FamilyName == element.GetFamilyName()))
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
        private void ProcessFamilyInstance(List<ExcelFamilyRecord> loadableFamilyDictionary,
            Document document, Element element, RevitSolidElement revitSolidElement)
        {
            var typeName = element.GetFamilyName();
            var record = loadableFamilyDictionary.FirstOrDefault(a => typeName.StartsWith(a.FamilyName.Substring(0, a.FamilyName.Length - 1)));
            if (record == null || element.GetFamilyCategory() != record.FamilyCategory)
            {
                revitSolidElement.IsNameCorrect = false;
            }
            else
            {
                revitSolidElement.IsNameCorrect = true;
                revitSolidElement.ElementName = record.ElementName;
                CheckParameters(record, document, element, revitSolidElement);
            }
        }
        private bool CheckRecordExtendName(ExcelFamilyRecord excelRecord, Element element)
        {
            const string typePrefix = "类型=";
            var recordExtendName = excelRecord.ExtendName;
            string incorrectMessage = $"补充属性不合理，族：{excelRecord.FamilyCategory} 类型：{excelRecord.FamilyName} 补充属性：{excelRecord.ExtendName}";
            if (recordExtendName.Contains("&&"))
            {
                var requires = recordExtendName.Split(new[] { "&&" }, StringSplitOptions.None);
                //存在多个条件时，
                return requires.All(a =>
                {
                    if (a.StartsWith(typePrefix))
                    {
                        return element.GetExtendName().StartsWith(a.Substring(3, a.Length - 4));
                    }
                    else
                    {
                        var str = a.Split('=');
                        if (str.Count() != 2)
                            TaskDialog.Show("错误信息", incorrectMessage);
                        var value = element.GetParameters(str[0]);
                        if (value.Count == 0)
                        {
                            return false;
                        }
                        return value[0]?.Element?.Name == str[1];
                    }
                });
            }
            else
            {
                if (recordExtendName.StartsWith(typePrefix))
                {
                    return element.GetExtendName().StartsWith(recordExtendName.Substring(3, recordExtendName.Length - 4));
                }
                else { TaskDialog.Show("错误信息", incorrectMessage); return false; }
            }
        }
        public bool CheckParameters(ExcelFamilyRecord excelRecord, Document document, Element element, RevitSolidElement revitSolidElement)
        {
            var familyElementID = element.LookupParameter("族与类型")?.AsElementId();
            var familyElement = document.GetElement(familyElementID);

            foreach (var propertyName in excelRecord.RequiredProperties)
            {
                var parameter = element.LookupParameter(propertyName.Value) ?? familyElement?.LookupParameter(propertyName.Value);
                revitSolidElement.Parameters.Add(new Parameter
                {
                    Name = propertyName.Value,
                    TDCName = propertyName.Key,
                    Value = parameter != null ? GetValue(parameter) : "缺失",
                    ValueType = parameter != null ? (parameter.Element.Id == element.Id ? "实例参数" : "类型参数") : ""
                });
            }
            revitSolidElement.IsPropertiesCorrect = revitSolidElement.Parameters.All(p => p.Value != "缺失");
            return revitSolidElement.IsPropertiesCorrect;
        }
        private string GetValue(Autodesk.Revit.DB.Parameter parameter)
        {
            switch (parameter.StorageType)
            {
                case StorageType.String:
                    return parameter.AsString();
                case StorageType.Double:
                    return parameter.AsValueString();
                case StorageType.Integer:
                    return parameter.AsValueString();
                case StorageType.ElementId:
                    return parameter.AsInteger().ToString();
                default:
                    return parameter.AsString();
            }
        }
    }
}
