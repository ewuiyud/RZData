using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    public class ElementData : ObservableObject
    {
        public ElementData()
        {
            Families = new ObservableCollection<Family>();
        }
        public ObservableCollection<Family> Families { get; set; }
        public void Add(Element element, ExcelRecord excelRecord)
        {
            if (Families.ToList().Exists(a => a.Name == element.Category.Name))
            {
                var family = Families.FirstOrDefault(a => a.Name == element.Category.Name);
                family.Add(element, excelRecord);
            }
            else
            {
                var family = new Family();
                family.Name = element.Category.Name;
                family.Add(element, excelRecord);
                Families.Add(family);
            }
        }
        public void Add(Element element)
        {
            if (Families.ToList().Exists(a => a.Name == element.Category.Name))
            {
                var family = Families.FirstOrDefault(a => a.Name == element.Category.Name);
                family.Add(element);
            }
            else
            {
                var family = new Family();
                family.Name = element.Category.Name;
                family.Add(element);
                Families.Add(family);
            }
        }

        internal void Clear()
        {
            this.Families = new ObservableCollection<Family>();
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
        public void Add(Element element, ExcelRecord excelRecord)
        {
            if (FamilyTypes.Any(a => a.Name == element.Category.Name))
            {
                FamilyTypes.First(a => a.Name == element.Category.Name).Add(element, excelRecord);
            }
            else
            {
                FamilyType familyType = new FamilyType();
                familyType.Name = element.Category.Name;
                familyType.Add(element, excelRecord);
                FamilyTypes.Add(familyType);
            }
        }

        internal void Add(Element element)
        {
            if (FamilyTypes.Any(a => a.Name == (element as FamilySymbol).FamilyName))
            {
                FamilyTypes.First(a => a.Name == (element as FamilySymbol).FamilyName).Add(element);
            }
            else
            {
                FamilyType familyType = new FamilyType();
                familyType.Name = (element as FamilySymbol).FamilyName;
                familyType.Add(element);
                FamilyTypes.Add(familyType);
            }
        }
    }
    public class FamilyType
    {
        public FamilyType()
        {
            FamilyExtends = new ObservableCollection<FamilyExtend>();
        }
        public string Name { get; set; }
        public ObservableCollection<FamilyExtend> FamilyExtends { get; set; }
        public void Add(Element element, ExcelRecord excelRecord)
        {
            FamilyExtend familyExtend = new FamilyExtend();
            familyExtend.ID = element.Id;
            familyExtend.Name = element.Name;
            foreach (var item in excelRecord.RequiredProperties)
            {
                var parameter = element.LookupParameter(item);
                if (parameter != null)
                {
                    familyExtend.Parameters.Add(new Parameter { Name = item, Value = "正常" });
                }
                else
                {
                    familyExtend.Parameters.Add(new Parameter { Name = item, Value = "缺失" });
                }
            }
            FamilyExtends.Add(familyExtend);
        }

        internal void Add(Element element)
        {
            FamilyExtend familyExtend = new FamilyExtend();
            familyExtend.ID = element.Id;
            familyExtend.Name = element.Name;
            FamilyExtends.Add(familyExtend);
        }
    }
    public class FamilyExtend
    {
        public FamilyExtend()
        {
            Parameters = new List<Parameter>();
        }
        public string Name { get; set; }
        public ElementId ID { get; set; }
        public List<Parameter> Parameters { get; set; }
    }
    public class Parameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
