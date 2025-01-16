using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace RZData.ViewModels
{
    public class RevitDataEntryViewModel : ObservableObject
    {
        private DataElementData _allElements;
        private object _selectedItem;
        private UIDocument _uiDocument;

        public ICommand SearchCommand { get; }

        private string _searchKeyword;
        private DataElementData _showElements;
        public DataElementData AllElements { get => _allElements; set => SetProperty(ref _allElements, value); }
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }
        public DataElementData ShowElements { get => _showElements; set => SetProperty(ref _showElements, value); }
        public RevitDataEntryViewModel(UIDocument _uiDocument, DataElementData AllElements)
        {
            this.AllElements = AllElements;
            this.ShowElements = AllElements;
            this._uiDocument = _uiDocument;
            SearchCommand = new RelayCommand(Search);
        }

        internal void DoubleClickAndPickObjects(object selectedValue)
        {
            switch (selectedValue)
            {
                case Models.Family family:
                    SelectElementInRevit(family);
                    break;
                case Models.FamilyType familyType:
                    SelectElementInRevit(familyType);
                    break;
                case FamilyExtend familyExtend:
                    SelectElementInRevit(familyExtend);
                    break;
                default:
                    break;
            }
        }
        private void SelectElementInRevit(Models.Family family)
        {
            var uidoc = _uiDocument;
            var elementIds = new List<ElementId>();
            foreach (var familyType in family.FamilyTypes)
            {
                foreach (var familyExtend in familyType.FamilyExtends)
                {
                    foreach (var item in familyExtend.DataInstances)
                    {
                        elementIds.Add(item.Element.Id);
                    }
                }
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(Models.FamilyType familyType)
        {
            var uidoc = _uiDocument;
            var elementIds = new List<ElementId>();
            foreach (var familyExtend in familyType.FamilyExtends)
            {
                foreach (var item in familyExtend.DataInstances)
                {
                    elementIds.Add(item.Element.Id);
                }
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(FamilyExtend familyExtend)
        {
            var uidoc = _uiDocument;
            var elementIds = new List<ElementId>();
            foreach (var item in familyExtend.DataInstances)
            {
                elementIds.Add(item.Element.Id);
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void Search()
        {
            ShowElements = AllElements.Search(SearchKeyword);
        }
    }
}
