using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class ElementViewModel : ObservableObject
    {
        public ElementViewModel(List<RevitSolidElement> revitSolidElements)
        {
            familyCategories = new ObservableCollection<FamilyCategoryViewModel>();
            RevitSolidElements = revitSolidElements;
            foreach (var item in RevitSolidElements)
            {
                Add(item);
            }
        }
        public List<RevitSolidElement> RevitSolidElements { get; set; }
        private ObservableCollection<FamilyCategoryViewModel> familyCategories;
        public ObservableCollection<FamilyCategoryViewModel> FamilyCategories { get => familyCategories; set => SetProperty(ref familyCategories, value); }
        private void Add(RevitSolidElement revitSolidElement)
        {
            var existingCategory = familyCategories.FirstOrDefault(a => a.Name == revitSolidElement.FamilyCategory);
            if (existingCategory == null)
            {
                var newCategory = new FamilyCategoryViewModel { Name = revitSolidElement.FamilyCategory };
                familyCategories.Add(newCategory);
                existingCategory = newCategory;
            }
            if (!existingCategory.IDs.Contains(revitSolidElement.ID)) existingCategory.IDs.Add(revitSolidElement.ID);

            var existingFamily = existingCategory.Families.FirstOrDefault(f => f.Name == revitSolidElement.FamilyName);
            if (existingFamily == null)
            {
                var newFamily = new FamilyViewModel { Name = revitSolidElement.FamilyName };
                existingCategory.Families.Add(newFamily);
                existingFamily = newFamily;
            }
            if (!existingFamily.IDs.Contains(revitSolidElement.ID)) existingFamily.IDs.Add(revitSolidElement.ID);
            //将族中的实例添加到族中
            var existingElementInstance = existingFamily.ElementInstances.FirstOrDefault(e => e.Name == revitSolidElement.ID);
            if (existingElementInstance == null)
            {
                var newElementInstance = new ElementInstanceViewModel { Name = revitSolidElement.ID, Parameters = revitSolidElement.Parameters };
                existingFamily.ElementInstances.Add(newElementInstance);
                existingElementInstance = newElementInstance;
            }

            //如果是系统族，需要添加族类型
            if (revitSolidElement.RevitElementFamilyType == RevitElementFamilyType.SystemFamilyElement)
            {
                var existingExtend = existingFamily.FamilyExtends.
                    FirstOrDefault(e => e.Name == revitSolidElement.ExtendName);
                if (existingExtend == null)
                {
                    var newExtend = new FamilyExtendViewModel { Name = revitSolidElement.ExtendName };
                    existingFamily.FamilyExtends.Add(newExtend);
                    existingExtend = newExtend;
                }
                if (!existingExtend.ElementInstances.Contains(existingElementInstance)) existingExtend.ElementInstances.Add(existingElementInstance);
                if (!existingExtend.IDs.Contains(revitSolidElement.ID)) existingExtend.IDs.Add(revitSolidElement.ID);

                //将族中的参数添加到族中
                foreach (var item in revitSolidElement.Parameters)
                {
                    var existingParameter = existingExtend.Parameters.FirstOrDefault(p => p.Name == item.Name);
                    if (existingParameter == null)
                    {
                        var newParameter = new Models.ParameterSet { Name = item.Name, Values = new ObservableCollection<string> { item.Value }, ValueType = item.ValueType };
                        existingExtend.Parameters.Add(newParameter);
                    }
                    else
                    {
                        //如果已经存在，需要判断是否已经添加过
                        if (!existingParameter.Values.Contains(item.Value))
                            existingParameter.Values.Add(item.Value);
                    }
                }
            }
            else
            {             //将族中的参数添加到族中
                foreach (var item in revitSolidElement.Parameters)
                {
                    var existingParameter = existingFamily.Parameters.FirstOrDefault(p => p.Name == item.Name);
                    if (existingParameter == null)
                    {
                        var newParameter = new Models.ParameterSet { Name = item.Name, Values = new ObservableCollection<string> { item.Value }, ValueType = item.ValueType };
                        existingFamily.Parameters.Add(newParameter);
                    }
                    else
                    {
                        //如果已经存在，需要判断是否已经添加过
                        if (!existingParameter.Values.Contains(item.Value))
                            existingParameter.Values.Add(item.Value);
                    }
                }
            }
        }
    }
}
