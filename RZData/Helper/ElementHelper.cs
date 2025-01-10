using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Helper
{
    public static class ElementHelper
    {
        public static string GetFamily(this Element element)
        {
            return element.Category?.Name ?? null;
        }
        public static string GetFamilyType(this Element element)
        {
            if (element is ElementType elementType)
            {
                return elementType.FamilyName;
            }
            else if (element is FamilyInstance familyInstance)
            {
                return familyInstance.Symbol.FamilyName;
            }
            else
            {
                var dd = element.GetParameters("族");
                return dd[0].Element.Category?.Name ?? null;
            }
        }
        public static string GetExtendName(this Element element)
        {
            return element.Name;
        }
    }
}
