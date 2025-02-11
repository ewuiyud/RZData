using Autodesk.Revit.DB;
using RZData.Extensions;
using RZData.Services;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace RZData.Models
{
    public class RevitSolidElement
    {
        public RevitSolidElement(Element element,
            RevitElementFamilyType revitElementFamilyType = RevitElementFamilyType.SystemFamilyElement)
        {
            ID = element.Id.IntegerValue;
            RevitElementFamilyType = revitElementFamilyType;
            FamilyCategory = element.GetFamilyCategory();
            FamilyName = element.GetFamilyName();
            ExtendName = element.GetExtendName();
            Parameters = new List<Parameter>();
        }
        public readonly RevitElementFamilyType RevitElementFamilyType;
        public string FamilyCategory { get; set; }
        public string FamilyName { get; set; }
        public string ExtendName { get; set; }
        public int ID { get; set; }
        public bool IsNameCorrect { get; set; }
        public List<Parameter> Parameters { get; set; }
        public bool IsPropertiesCorrect { get; set; }
        public string ElementName { get; set; }
    }
    public enum RevitElementFamilyType
    {
        SystemFamilyElement,
        LoadFamilyElement
    }
}
