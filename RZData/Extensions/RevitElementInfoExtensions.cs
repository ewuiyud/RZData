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
            var familyNames = element.GetParameters("族");
            if (familyNames.Count == 0)
            {
                return null;
            }
            return familyNames[0].AsValueString() ?? null;
        }
        public static string GetExtendName(this Element element)
        {
            return element.Name;
        }
        public static string GetElementValue(this Element element, Document document, string parameterName)
        {
            var familyElementID = element.LookupParameter("族与类型")?.AsElementId();
            var familyElement = document.GetElement(familyElementID);
            var parameter = element.LookupParameter(parameterName) ?? familyElement?.LookupParameter(parameterName);

            return parameter.GetValue();
        }

        /// <summary>
        /// 允许使用通配符*，str1为
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static bool IsSameAs(this string str1, string str2)
        {
            if (str1 == str2)
            {
                return true;
            }
            if (str1.EndsWith("*"))
            {
                if (str2.StartsWith(str1.Substring(0, str1.Length - 1)))
                {
                    return true;
                }
            }
            if (str2.EndsWith("*"))
            {
                if (str1.StartsWith(str2.Substring(0, str2.Length - 1)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
