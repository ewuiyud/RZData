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
            if (parameter is null)
            {
                return null;
            }
            string result = null;
            switch (parameter.StorageType)
            {
                case StorageType.String:
                    result = parameter.AsString();
                    break;
                case StorageType.Double:
                    result = parameter.AsValueString();
                    break;
                case StorageType.Integer:
                    result = parameter.AsValueString();
                    break;
                case StorageType.ElementId:
                    result = parameter.AsInteger().ToString();
                    break;
                default:
                    result = parameter.AsString();
                    break;
            }
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            return result;
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
