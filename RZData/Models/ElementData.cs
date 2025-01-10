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

namespace RZData.Models
{
    public class ElementData : ObservableObject
    {
        public ElementData()
        {
            Families = new ObservableCollection<Family>();
        }
        public ObservableCollection<Family> Families { get; set; }

        public void Add(Element element)
        {
            var family = Families.FirstOrDefault(a => a.Name == element.GetFamily());
            if (family == null)
            {
                family = new Family { Name = element.GetFamily() };
                Families.Add(family);
            }
            family.Add(element);
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

        public void Add(Element element)
        {
            if (element.GetFamilyType() == null) return;

            var familyType = FamilyTypes.FirstOrDefault(a => a.Name == element.GetFamilyType());
            if (familyType == null)
            {
                familyType = new FamilyType { Name = element.GetFamilyType() };
                FamilyTypes.Add(familyType);
            }
            familyType.Add(element);
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
        internal void Add(Element element)
        {
            FamilyExtend familyExtend = new FamilyExtend();
            familyExtend.Element = element;
            familyExtend.Name = element.GetExtendName();
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
        public Element Element { get; set; }
        public List<Parameter> Parameters { get; set; }
    }
    public class Parameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
