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
            children = new ObservableCollection<FamilyViewModel>();
        }
        public string Name { get; set; }
        private ObservableCollection<FamilyViewModel> children;
        public ObservableCollection<FamilyViewModel> Children { get => children; set => SetProperty(ref children, value); }

        private bool? isChecked = false;
        public bool? IsChecked { get => isChecked; set => SetProperty(ref isChecked, value); }
        public void ResetIsChecked()
        {
            var allElementInstacees = GetAllElementInstanceViewModels();
            bool allChecked = allElementInstacees.TrueForAll(a => a.IsChecked);
            bool allUnchecked = allElementInstacees.TrueForAll(a => !a.IsChecked);
            if (allChecked)
            {
                IsChecked = true;
            }
            else if (allUnchecked)
            {
                IsChecked = false;
            }
            else
            {
                IsChecked = null;
            }
        }
        /// <summary>
        /// 获取所有的ElementInstanceViewModel
        /// </summary>
        /// <returns></returns>
        public List<ElementInstanceViewModel> GetAllElementInstanceViewModels()
        {
            var result = new List<ElementInstanceViewModel>();
            foreach (var family in Children)
            {
                foreach (var familyExtend in family.Children)
                {
                    foreach (var elementInstance in familyExtend.Children)
                    {
                        result.Add(elementInstance);
                    }
                }
            }
            return result;
        }
    }
}
