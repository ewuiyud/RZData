using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Extensions;
using RZData.ViewModels;
using System.Collections.Generic;

namespace RZData.Models
{
    public class RevitSolidElement
    {
        public RevitSolidElement()
        {
            ID = 0;
            RevitElementFamilyType = RevitElementFamilyType.SystemFamilyElement;
            FamilyCategory = string.Empty;
            FamilyName = string.Empty;
            ExtendName = string.Empty;
            Parameters = new List<ParameterVM>();
        }
        public RevitSolidElement(Element element)
        {
            ID = element.Id.IntegerValue;
            if (element is FamilyInstance)
                RevitElementFamilyType = RevitElementFamilyType.LoadFamilyElement;
            else
                RevitElementFamilyType = RevitElementFamilyType.SystemFamilyElement;
            FamilyCategory = element.GetFamilyCategory();
            FamilyName = element.GetFamilyName();
            ExtendName = element.GetExtendName();
            Parameters = new List<ParameterVM>();
        }

        public RevitSolidElement(UIDocument uIDocument, ElementInstanceViewModel elementInstance)
        {
            ID = elementInstance.Id;
            var element = uIDocument.Document.GetElement(new ElementId(ID));
            if (element is FamilyInstance)
                RevitElementFamilyType = RevitElementFamilyType.LoadFamilyElement;
            else
                RevitElementFamilyType = RevitElementFamilyType.SystemFamilyElement;
            FamilyCategory = element.GetFamilyCategory();
            FamilyName = element.GetFamilyName();
            ExtendName = element.GetExtendName();
            Parameters = elementInstance.Parameters;
        }

        public readonly RevitElementFamilyType RevitElementFamilyType;
        public string FamilyCategory { get; set; }
        public string FamilyName { get; set; }
        public string ExtendName { get; set; }
        public int ID { get; set; }
        public bool IsNameCorrect { get; set; }
        public List<ParameterVM> Parameters { get; set; }
        public bool IsPropertiesCorrect { get; set; }
        public string ElementName { get; set; }
    }
    public enum RevitElementFamilyType
    {
        SystemFamilyElement,
        LoadFamilyElement
    }
}
