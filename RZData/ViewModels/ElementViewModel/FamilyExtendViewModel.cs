using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RZData.ViewModels
{
    public class FamilyExtendViewModel : ObservableObject
    {
        public FamilyExtendViewModel()
        {
            IDs = new List<int>();
            ElementInstances = new ObservableCollection<ElementInstanceViewModel>();
            Parameters = new List<ParameterSetVM>();
        }
        public FamilyViewModel Parent { get; set; }
        public string Name { get; set; }
        public List<int> IDs { get; set; }
        public ObservableCollection<ElementInstanceViewModel> ElementInstances { get; set; }
        public List<ParameterSetVM> Parameters { get; set; }
        internal void ResetParameter(Document document, ParameterSetVM parameter)
        {
            var name = parameter.Name; var value = parameter.Value;
            foreach (var ElementInstance in ElementInstances)
            {
                var p = ElementInstance.Parameters.FirstOrDefault(a => a.Name == name);
                if (p != null)
                {
                    p.Value = value;
                    p.IsModified = false;
                }
            }
        }
        internal void MergeParameters()
        {
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
                        Parameters.Add(new ParameterSetVM(parameter));
                    }
                }
            }
        }
    }
}
