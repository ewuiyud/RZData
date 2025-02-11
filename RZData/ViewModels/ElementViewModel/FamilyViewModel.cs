using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    }
}
