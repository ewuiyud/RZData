using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class FamilyViewModel : ObservableObject
    {
        public FamilyViewModel()
        {
            familyExtends = new ObservableCollection<FamilyExtendViewModel>();
            IDs = new List<int>();
            ElementInstances = new ObservableCollection<ElementInstanceViewModel>();
            Parameters = new List<ParameterSetVM>();
        }
        public string Name { get; set; }
        public List<int> IDs { get; set; }
        public ObservableCollection<ElementInstanceViewModel> ElementInstances { get; set; }
        public List<ParameterSetVM> Parameters { get; set; }
        private ObservableCollection<FamilyExtendViewModel> familyExtends;
        public ObservableCollection<FamilyExtendViewModel> FamilyExtends { get => familyExtends; set => SetProperty(ref familyExtends, value); }
        internal void ResetParameter(Document document, ParameterSetVM parameter)
        {
            var name = parameter.Name; var value = parameter.Value;
            //修改所有拓展类型的参数
            foreach (var familyExtend in FamilyExtends)
            {
                var p = familyExtend.Parameters.FirstOrDefault(a => a.Name == name);
                if (p != null)
                {
                    p.Value = value;
                    p.IsModified = false;
                }
                familyExtend.ResetParameter(document, parameter);
                parameter.IsModified = false;
            }
        }
        public void MergeParameters()
        {
            if (Parameters.Count == 0)
            {
                return;
            }
            Parameters.Clear();
            foreach (var ElementInstance in ElementInstances)
            {
                foreach (var parameter in ElementInstance.Parameters)
                {
                    var currentP = Parameters.FirstOrDefault(a => a.Name == parameter.Name);
                    if (currentP != null)
                    {
                        if (!currentP.Parameters.Contains(parameter))
                        {
                            currentP.Parameters.Add(parameter);
                        }
                    }
                    else
                    {
                        var parameterSet = new ParameterSetVM(parameter);
                        parameterSet.Parameters.Add(parameter);
                        Parameters.Add(parameterSet);
                    }
                }
            }
        }
    }
}
