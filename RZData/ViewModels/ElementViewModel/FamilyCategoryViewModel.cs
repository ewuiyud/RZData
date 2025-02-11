using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class FamilyCategoryViewModel : ObservableObject
    {
        public FamilyCategoryViewModel()
        {
            families = new ObservableCollection<FamilyViewModel>();
            IDs = new List<int>();
        }
        public List<int> IDs { get; set; }
        public string Name { get; set; }
        private ObservableCollection<FamilyViewModel> families;
        public ObservableCollection<FamilyViewModel> Families { get => families; set => SetProperty(ref families, value); }
    }
}
