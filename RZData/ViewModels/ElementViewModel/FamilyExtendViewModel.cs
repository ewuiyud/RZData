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
            Children = new ObservableCollection<ElementInstanceViewModel>();
        }
        public FamilyViewModel Parent { get; set; }
        public string Name { get; set; }
        public List<int> IDs { get; set; }
        public ObservableCollection<ElementInstanceViewModel> Children { get; set; }

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
            return Children.ToList();
        }
    }
}
