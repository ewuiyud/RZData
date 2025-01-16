using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace RZData.ViewModels
{
    public class RevitDataEntryViewModel : ObservableObject
    {
        private DataElementData _allElements;
        private object _selectedItem;
        private UIDocument _uiDocument;
        private string _searchKeyword;
        private DataElementData _showElements;
        private Models.Family _selectedElement;
        public ICommand SearchCommand { get; }
        public ICommand OKCommand { get; }
        public DataElementData AllElements { get => _allElements; set => SetProperty(ref _allElements, value); }
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }
        public DataElementData ShowElements { get => _showElements; set => SetProperty(ref _showElements, value); }
        public ObservableCollection<Models.Family> Families
        {
            get
            {
                var fs = new ObservableCollection<Models.Family>();
                fs.Add(new Models.Family() { Name = "所有" });
                AllElements.Families.ToList().ForEach(a => fs.Add(a));
                return fs;
            }
        }
        public Models.Family SelectedElement
        {
            get => _selectedElement;
            set
            {
                if (_selectedElement != value)
                {
                    _selectedElement = value;
                    OnPropertyChanged(nameof(SelectedElement));

                    if (_selectedElement.Name == "所有")
                    {
                        ShowElements = AllElements;
                    }
                    else
                    {
                        ShowElements = new DataElementData() { Families = new ObservableCollection<Models.Family> { _selectedElement} };
                    }
                }
            }
        }
        public RevitDataEntryViewModel(UIDocument _uiDocument, DataElementData AllElements)
        {
            this.AllElements = AllElements;
            this.ShowElements = AllElements;
            this._uiDocument = _uiDocument;
            SearchCommand = new RelayCommand(Search);
            OKCommand = new RelayCommand(OK);
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
        private void OK()
        {
            if (SelectedItem is FamilyExtend familyExtend)
            {
                foreach (var parameter in familyExtend.Parameters)
                {
                    SetParameter(parameter, familyExtend.DataInstances);
                }
            }
        }

        private void SetParameter(Models.ParameterSet parameterSet, ObservableCollection<DataInstance> dataInstances)
        {
            if (parameterSet.ValueType == "实例参数")
            {
                foreach (var dataInstance in dataInstances)
                {
                    Element element = dataInstance.Element;
                    var p = element.LookupParameter(parameterSet.Name);
                    if (!p.IsReadOnly && !p.Set(parameterSet.Value))
                    {
                        TaskDialog.Show("错误报告", $"输入参数的值不合法，参数 {parameterSet.Name} 的值 {parameterSet.Value}");
                    }
                }
            }
            else if (parameterSet.ValueType == "类型参数")
            {
                Element element = _uiDocument.Document.GetElement(dataInstances[0].Element.LookupParameter("族与类型")?.AsElementId());
                var p = element.LookupParameter(parameterSet.Name);
                if (!p.IsReadOnly && !p.Set(parameterSet.Value))
                {
                    TaskDialog.Show("错误报告", $"输入参数的值不合法，参数 {parameterSet.Name} 的值 {parameterSet.Value}");
                }
            }

        }
    }
}
