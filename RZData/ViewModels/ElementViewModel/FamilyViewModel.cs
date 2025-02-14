using Autodesk.Revit.DB;
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
            Parameters = new List<Models.ParameterSet>();
        }
        public string Name { get; set; }
        public List<int> IDs { get; set; }
        public ObservableCollection<ElementInstanceViewModel> ElementInstances { get; set; }
        public List<Models.ParameterSet> Parameters { get; set; }
        private ObservableCollection<FamilyExtendViewModel> familyExtends;
        public ObservableCollection<FamilyExtendViewModel> FamilyExtends { get => familyExtends; set => SetProperty(ref familyExtends, value); }

        internal void ReloadParameter(Document document)
        {
            foreach (var ElementInstance in ElementInstances)
            {
                ElementInstance.ReloadParameter(document);
            }
            MergeParameters();
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
                        var parameterSet = new Models.ParameterSet(parameter);
                        parameterSet.Parameters.Add(parameter);
                        Parameters.Add(parameterSet);
                    }
                }
            }
        }
    }
}
