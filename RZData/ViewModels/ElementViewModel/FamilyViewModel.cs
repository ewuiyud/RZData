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
            children = new ObservableCollection<FamilyExtendViewModel>();
            IDs = new List<int>();
            ElementInstances = new ObservableCollection<ElementInstanceViewModel>();
        }
        public string Name { get; set; }
        public List<int> IDs { get; set; }
        public FamilyCategoryViewModel Parent { get; set; }
        public ObservableCollection<ElementInstanceViewModel> ElementInstances { get; set; }
        private ObservableCollection<FamilyExtendViewModel> children;
        public ObservableCollection<FamilyExtendViewModel> Children { get => children; set => SetProperty(ref children, value); }

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

            foreach (var familyExtend in Children)
            {
                foreach (var elementInstance in familyExtend.Children)
                {
                    result.Add(elementInstance);
                }
            }
            return result;
        }
    }
}
