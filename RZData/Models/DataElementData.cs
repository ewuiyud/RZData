﻿using Autodesk.Revit.DB;
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

namespace RZData.Models
{
    public class DataElementData : ObservableObject
    {
        private ObservableCollection<Family> _families;
        public DataElementData()
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
        public bool IsPropertiesCorrect { get; set; }
        public ObservableCollection<DataInstance> DataInstances { get; set; }
        public ObservableCollection<ParameterSet> Parameters { get; set; }
        internal void MergeParameters()
        {
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
                        Parameters.Add(new ParameterSet() { Values = new List<string> { parameter.Value }, Name = parameter.Name });
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
        public ObservableCollection<Parameter> Parameters { get; set; }
        public bool CheckParameters(ExcelRecord excelRecord)
        {
            excelRecord.RequiredProperties.ForEach(a =>
            {
                Parameters.Add(new Parameter
                {
                    Name = a,
                    Value = Element.GetParameters(a).Count() > 0 ? Element.GetParameters(a).First().AsString() : "缺失"
                });
            });
            IsPropertiesCorrect = Parameters.All(a => a.Value != "缺失");
            return IsPropertiesCorrect;
        }
    }
    public class Parameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class ParameterSet
    {
        public string Name { get; set; }
        public List<string> Values { get; set; }
        public string Value
        {
            get
            {
                if (Values.Count == 1)
                {
                    return Values[0];
                }
                else
                {
                    return $"[{string.Join(", ", Values)}]";
                }
            }
        }
    }
}
