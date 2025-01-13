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
            var dd = element.GetParameters("族");
            return dd[0].AsValueString() ?? null;
        }
        public static string GetExtendName(this Element element)
        {
            return element.Name;
        }
    }
}
