using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Extensions
{
    public static class RevitElementInfoExtensions
    {
        public static string GetValue(this Autodesk.Revit.DB.Parameter parameter)
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
        public static string GetFamilyCategory(this Element element)
        {
            return element.Category?.Name ?? null;
        }
        public static string GetFamilyName(this Element element)
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
