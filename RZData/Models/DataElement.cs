﻿using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using RZData.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using static OfficeOpenXml.ExcelErrorValue;

namespace RZData.Models
{
    public class DataElement : ObservableObject
    {
        private ObservableCollection<Family> _families;
        public DataElement()
        {
            Families = new ObservableCollection<Family>();
        }
        public ObservableCollection<Family> Families
        {
            get => _families;
            set => SetProperty(ref _families, value);
        }

        public DataInstance Add(Element element)
        {
            var family = Families.FirstOrDefault(a => a.Name == element.GetFamily());
            if (family == null)
            {
                family = new Family { Name = element.GetFamily() };
                Families.Add(family);
            }
            return family.Add(element);
        }

        internal void Clear()
        {
            Families = new ObservableCollection<Family>();
        }
        internal void MergeParameters()
        {
            foreach (var family in Families)
            {
                foreach (var familyType in family.FamilyTypes)
                {
                    foreach (var familyExtend in familyType.FamilyExtends)
                    {
                        familyExtend.MergeParameters();
                    }
                }
            }
        }

        public void Add(DataInstance dataInstance)
        {
            var e = Add(dataInstance.Element);
            e.Parameters = dataInstance.Parameters;
        }

        internal DataElement Search(string searchKeyword)
        {
            var result = new DataElement();
            foreach (var family in Families)
            {
                if (family.Name.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.Families.Add(family);
                    continue;
                }

                var newFamily = new Family { Name = family.Name };
                foreach (var familyType in family.FamilyTypes)
                {
                    if (familyType.Name.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        newFamily.FamilyTypes.Add(familyType);
                        continue;
                    }

                    var newFamilyType = new FamilyType(newFamily) { Name = familyType.Name };
                    foreach (var familyExtend in familyType.FamilyExtends)
                    {
                        if (familyExtend.Name.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            newFamilyType.FamilyExtends.Add(familyExtend);
                        }
                    }

                    if (newFamilyType.FamilyExtends.Any())
                    {
                        newFamily.FamilyTypes.Add(newFamilyType);
                    }
                }

                if (newFamily.FamilyTypes.Any())
                {
                    result.Families.Add(newFamily);
                }
            }

            return result;
        }
    }
    public class Family
    {
        public Family()
        {
            FamilyTypes = new ObservableCollection<FamilyType>();
        }
        public string Name { get; set; }
        public ObservableCollection<FamilyType> FamilyTypes { get; set; }

        public DataInstance Add(Element element)
        {
            if (element.GetFamilyType() == null) return null;

            var familyType = FamilyTypes.FirstOrDefault(a => a.Name == element.GetFamilyType());
            if (familyType == null)
            {
                familyType = new FamilyType(this) { Name = element.GetFamilyType() };
                FamilyTypes.Add(familyType);
            }
            return familyType.Add(element);
        }
    }
    public class FamilyType
    {
        public Family Family { get; set; }
        public FamilyType(Family family)
        {
            FamilyExtends = new ObservableCollection<FamilyExtend>();
            Family = family;
        }
        public string Name { get; set; }
        public ObservableCollection<FamilyExtend> FamilyExtends { get; set; }

        internal DataInstance Add(Element element)
        {
            var familyExtend = FamilyExtends.FirstOrDefault(a => a.Name == element.GetExtendName());
            if (familyExtend == null)
            {
                familyExtend = new FamilyExtend(element, this);
                FamilyExtends.Add(familyExtend);
            }
            return familyExtend.Add(element);
        }
    }
    public class FamilyExtend
    {
        public FamilyType FamilyType { get; set; }
        public string Name { get; set; }
        public FamilyExtend(Element element, FamilyType familyType)
        {
            DataInstances = new ObservableCollection<DataInstance>();
            Parameters = new ObservableCollection<ParameterSet>();
            Name = element.GetExtendName();
            FamilyType = familyType;
        }
        public bool IsNameCorrect { get; set; }
        public ObservableCollection<DataInstance> DataInstances { get; set; }
        public ObservableCollection<ParameterSet> Parameters { get; set; }
        internal void MergeParameters()
        {
            Parameters.Clear();
            foreach (var dataInstance in DataInstances)
            {
                foreach (var parameter in dataInstance.Parameters)
                {
                    var currentP = Parameters.FirstOrDefault(a => a.Name == parameter.Name);
                    if (currentP != null)
                    {
                        if (!currentP.Values.Contains(parameter.Value))
                        {
                            currentP.Values.Add(parameter.Value);
                        }
                    }
                    else
                    {
                        Parameters.Add(new ParameterSet() { Values = new List<string> { parameter.Value }, Name = parameter.Name, ValueType = parameter.ValueType });
                    }
                }
            }
        }
        public DataInstance Add(Element element)
        {
            var dataInstance = new DataInstance(element, this);
            DataInstances.Add(dataInstance);
            return dataInstance;
        }

        internal void ReloadParameter(Autodesk.Revit.DB.Document document)
        {
            foreach (var dataInstance in DataInstances)
            {
                dataInstance.ReloadParameter(document);
            }
            MergeParameters();
        }
    }
    public class DataInstance
    {
        public FamilyExtend FamilyExtend { get; set; }
        public DataInstance(Element element, FamilyExtend familyExtend)
        {
            Parameters = new ObservableCollection<Parameter>();
            Element = element;
            FamilyExtend = familyExtend;
        }
        public bool IsPropertiesCorrect { get; set; }
        public Element Element { get; set; }
        /// <summary>
        /// 元素分类名称
        /// </summary>
        public string ElementName { get; set; }
        public ObservableCollection<Parameter> Parameters { get; set; }
        public bool CheckParameters(ExcelFamilyRecord excelRecord, Autodesk.Revit.DB.Document document)
        {
            var familyElementID = Element.LookupParameter("族与类型")?.AsElementId();
            var familyElement = document.GetElement(familyElementID);

            foreach (var propertyName in excelRecord.RequiredProperties)
            {
                var parameter = Element.LookupParameter(propertyName.Value) ?? familyElement?.LookupParameter(propertyName.Value);
                Parameters.Add(new Parameter
                {
                    Name = propertyName.Value,
                    TDCName = propertyName.Key,
                    Value = parameter != null ? GetValue(parameter) : "缺失",
                    ValueType = parameter != null ? (parameter.Element.Id == Element.Id ? "实例参数" : "类型参数") : ""
                });
            }
            ElementName = excelRecord.ElementName;
            IsPropertiesCorrect = Parameters.All(p => p.Value != "缺失");
            return IsPropertiesCorrect;
        }
        public void ReloadParameter(Autodesk.Revit.DB.Document document)
        {
            var familyElementID = Element.LookupParameter("族与类型")?.AsElementId();
            var familyElement = document.GetElement(familyElementID);
            foreach (var p in Parameters)
            {
                var parameter = Element.LookupParameter(p.Name) ?? familyElement?.LookupParameter(p.Name);
                p.Value = parameter != null ? GetValue(parameter) : "缺失";
                p.ValueType = parameter != null ? (parameter.Element.Id == Element.Id ? "实例参数" : "类型参数") : "";
            }
            IsPropertiesCorrect = Parameters.All(p => p.Value != "缺失");
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

    public class Parameter
    {
        public string Name { get; set; }
        public string TDCName { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
    }
    public class ParameterSet : ObservableObject
    {
        private string _value;
        public string Name { get; set; }
        public List<string> Values { get; set; }
        public string ValueType { get; set; }
        public string Status
        {
            get
            {
                if (Values.Count == 1)
                {
                    return "";
                }
                else
                {
                    return "多参数";
                }
            }
        }
        public string Value
        {
            get
            {
                if (_value == null)
                    if (Values.Count == 1)
                    {
                        return Values[0];
                    }
                    else
                    {
                        return $"[{string.Join(", ", Values)}]";
                    }
                return _value;
            }
            set => SetProperty(ref _value, value);
        }
        public string ShowValue
        {
            get
            {
                if (Value == "缺失")
                {
                    return "缺失";
                }
                else
                {
                    return "正常";
                }
            }
        }
    }
}
