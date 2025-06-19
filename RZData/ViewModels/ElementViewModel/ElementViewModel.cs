using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RZData.ViewModels
{
    public class ElementViewModel : ObservableObject
    {
        public ElementViewModel(List<RevitSolidElement> revitSolidElements)
        {
            Children = new ObservableCollection<FamilyCategoryViewModel>();
            RevitSolidElements = revitSolidElements;
            foreach (var item in RevitSolidElements)
            {
                Add(item);
            }
        }
        public List<RevitSolidElement> RevitSolidElements { get; set; }
        private ObservableCollection<FamilyCategoryViewModel> children;
        public ObservableCollection<FamilyCategoryViewModel> Children { get => children; set => SetProperty(ref children, value); }
        private void Add(RevitSolidElement revitSolidElement)
        {
            var existingCategory = children.FirstOrDefault(a => a.Name == revitSolidElement.FamilyCategory);
            if (existingCategory == null)
            {
                var newCategory = new FamilyCategoryViewModel { Name = revitSolidElement.FamilyCategory };
                children.Add(newCategory);
                existingCategory = newCategory;
            }

            var existingFamily = existingCategory.Children.FirstOrDefault(f => f.Name == revitSolidElement.FamilyName);
            if (existingFamily == null)
            {
                var newFamily = new FamilyViewModel { Name = revitSolidElement.FamilyName, Parent = existingCategory };
                existingCategory.Children.Add(newFamily);
                existingFamily = newFamily;
            }
            if (!existingFamily.IDs.Contains(revitSolidElement.ID)) existingFamily.IDs.Add(revitSolidElement.ID);
            //将族中的实例添加到族中
            var existingElementInstance = existingFamily.ElementInstances.FirstOrDefault(e => e.Id == revitSolidElement.ID);
            if (existingElementInstance == null)
            {
                var newElementInstance = new ElementInstanceViewModel
                {
                    Id = revitSolidElement.ID,
                    Parameters = revitSolidElement.Parameters,
                };
                existingFamily.ElementInstances.Add(newElementInstance);
                existingElementInstance = newElementInstance;
            }

            var existingExtend = existingFamily.Children.
                FirstOrDefault(e => e.Name == revitSolidElement.ExtendName);
            if (existingExtend == null)
            {
                var newExtend = new FamilyExtendViewModel { Name = revitSolidElement.ExtendName, Parent = existingFamily };
                existingFamily.Children.Add(newExtend);
                existingExtend = newExtend;
            }
            if (!existingExtend.Children.Contains(existingElementInstance))
            {
                existingExtend.Children.Add(existingElementInstance);
                existingElementInstance.Parent = existingExtend;
            }
            if (!existingExtend.IDs.Contains(revitSolidElement.ID)) existingExtend.IDs.Add(revitSolidElement.ID);
        }
        //全选或者全不选
        internal void SelectAll(bool IsSelected)
        {
            foreach (var category in children)
            {
                category.IsChecked = IsSelected;
                foreach (var family in category.Children)
                {
                    family.IsChecked = IsSelected;
                    foreach (var extend in family.Children)
                    {
                        extend.IsChecked = IsSelected;
                        foreach (var elementInstance in extend.Children)
                        {
                            elementInstance.IsChecked = IsSelected;
                        }
                    }
                }
            }
        }
        //获取所有的ElementInstanceViewModel
        internal List<ElementInstanceViewModel> GetAllElements()
        {
            var result = new List<ElementInstanceViewModel>();
            foreach (var category in children)
            {
                foreach (var family in category.Children)
                {
                    foreach (var extend in family.Children)
                    {
                        foreach (var elementInstance in extend.Children)
                        {
                            result.Add(elementInstance);
                        }
                    }
                }
            }
            return result.Distinct().ToList();
        }

        internal void SelectObject(object obj)
        {
            if (obj is FamilyCategoryViewModel familyCategory)
            {
                familyCategory.IsChecked = true;
                foreach (var family in familyCategory.Children)
                {
                    family.IsChecked = true;
                    foreach (var familyExtend in family.Children)
                    {
                        familyExtend.IsChecked = true;
                    }
                }
                var elements = familyCategory.GetAllElementInstanceViewModels();
                elements.ForEach(e => e.IsChecked = true);
            }
            else if (obj is FamilyViewModel family)
            {
                family.IsChecked = true;
                var elements = family.GetAllElementInstanceViewModels();
                foreach (var familyExtend in family.Children)
                {
                    familyExtend.IsChecked = true;
                }
                elements.ForEach(e => e.IsChecked = true);
                family.Parent.ResetIsChecked();
            }
            else if (obj is FamilyExtendViewModel familyExtend)
            {
                familyExtend.IsChecked = true;
                var elements = familyExtend.GetAllElementInstanceViewModels();
                elements.ForEach(e => e.IsChecked = true);
                familyExtend.Parent.ResetIsChecked();
                familyExtend.Parent.Parent.ResetIsChecked();
            }
            else if (obj is ElementInstanceViewModel elementInstance)
            {
                elementInstance.IsChecked = true;
                elementInstance.Parent.ResetIsChecked();
                elementInstance.Parent.Parent.ResetIsChecked();
                elementInstance.Parent.Parent.Parent.ResetIsChecked();
            }
        }
    }
}
