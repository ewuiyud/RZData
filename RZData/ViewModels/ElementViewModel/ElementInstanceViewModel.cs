using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using RZData.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class ElementInstanceViewModel : ObservableObject
    {
        public ElementInstanceViewModel()
        {
            Parameters = new List<Models.Parameter>();
        }
        public int Name { get; set; }
        public List<Models.Parameter> Parameters { get; set; }

        public void ReloadParameter(Document document)
        {
            Element element = document.GetElement(new ElementId(Name));
            var familyElementID = element.LookupParameter("族与类型")?.AsElementId();
            var familyElement = document.GetElement(familyElementID);
            foreach (var p in Parameters)
            {
                var parameter = element.LookupParameter(p.Name) ?? familyElement?.LookupParameter(p.Name);
                p.Value = parameter != null ? parameter.GetValue() : "缺失";
                p.ValueType = parameter != null ? (parameter.Element.Id == element.Id ? "实例参数" : "类型参数") : "";
            }
        }
    }
}
