using Autodesk.Revit.DB;
using RZData.Extensions;
using RZData.Services;

namespace RZData.Models
{
    public class RevitSolidElement
    {
        public RevitSolidElement(Element element)
        {
            ID = element.Id.IntegerValue;
            FamilyCategory = element.GetFamilyCategory();
            FamilyName = element.GetFamilyName();
            ExtendName = element.GetExtendName();
        }
        public string FamilyCategory { get; set; }
        public string FamilyName { get; set; }
        public string ExtendName { get; set; }
        public int ID { get; set; }
    }
}
