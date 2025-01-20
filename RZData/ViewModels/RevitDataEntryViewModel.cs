using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using RZData.ExternalEventHandlers;
using RZData.Models;
using RZData.Views;
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
    public class RevitDataEntryViewModel : BaseViewModel
    {
        private object _selectedItem;
        private string _searchKeyword;
        private DataElement _showElements;
        private Models.Family _selectedElement;
        public ICommand SearchCommand { get; }
        public ICommand OKCommand { get; }
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public string SearchKeyword { get => _searchKeyword; set => SetProperty(ref _searchKeyword, value); }
        public DataElement ShowElements { get => _showElements; set => SetProperty(ref _showElements, value); }
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
                        ShowElements = new DataElement() { Families = new ObservableCollection<Models.Family> { _selectedElement } };
                    }
                }
            }
        }
        public RevitDataEntryViewModel(UIDocument _uiDocument, DataElement AllElements)
        {
            this.AllElements = AllElements;
            this.ShowElements = AllElements;
            this.UiDocument = _uiDocument;
            SearchCommand = new RelayCommand(Search);
            OKCommand = new AsyncRelayCommand(OK);
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
                case DataInstance dataInstance:
                    SelectElementInRevit(dataInstance);
                    break;
                default:
                    break;
            }
        }
        private void SelectElementInRevit(DataInstance dataInstance)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            elementIds.Add(dataInstance.Element.Id);
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(Models.Family family)
        {
            var uidoc = UiDocument;
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
            var uidoc = UiDocument;
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
            var uidoc = UiDocument;
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
        private async Task OK()
        {
            try
            {
                if (SelectedItem is FamilyExtend familyExtend)
                {
                    foreach (var parameter in familyExtend.Parameters)
                    {
                        if ( !parameter.Value.StartsWith("["))
                        {
                            await CustomHandler.Run(a =>
                            {
                                SetParameter(a.ActiveUIDocument, parameter, familyExtend.DataInstances);
                            });
                        }
                    }
                    familyExtend.ReloadParameter(UiDocument.Document);
                }
                else if (SelectedItem is DataInstance dataInstance)
                {
                    foreach (var parameter in dataInstance.Parameters)
                    {
                        await CustomHandler.Run(a =>
                        {
                            SetParameter(a.ActiveUIDocument, parameter, dataInstance);
                        });
                    }
                    dataInstance.FamilyExtend.ReloadParameter(UiDocument.Document);
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void SetParameter(UIDocument uIDocument, Models.Parameter parameter, DataInstance dataInstance)
        {
            using (Transaction transaction = new Transaction(uIDocument.Document, "SetParameter"))
            {
                transaction.Start();
                if (parameter.ValueType == "实例参数")
                {
                    Element element = dataInstance.Element;
                    var p = element.LookupParameter(parameter.Name);
                    if (!p.IsReadOnly && !p.Set(parameter.Value))
                    {
                        TaskDialog.Show("错误报告", $"输入参数的值不合法，参数 {parameter.Name} 的值 {parameter.Value}");
                    }
                }
                else if (parameter.ValueType == "类型参数")
                {
                    Element element = UiDocument.Document.GetElement(dataInstance.Element.LookupParameter("族与类型")?.AsElementId());
                    var p = element.LookupParameter(parameter.Name);
                    if (!p.IsReadOnly && !p.Set(parameter.Value))
                    {
                        TaskDialog.Show("错误报告", $"输入参数的值不合法，参数 {parameter.Name} 的值 {parameter.Value}");
                    }
                }
                transaction.Commit();
            }
        }

        private void SetParameter(UIDocument uIDocument, Models.ParameterSet parameterSet, ObservableCollection<DataInstance> dataInstances)
        {
            using (Transaction transaction = new Transaction(uIDocument.Document, "SetParameter"))
            {
                transaction.Start();
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
                    Element element = UiDocument.Document.GetElement(dataInstances[0].Element.LookupParameter("族与类型")?.AsElementId());
                    var p = element.LookupParameter(parameterSet.Name);
                    if (!p.IsReadOnly && !p.Set(parameterSet.Value))
                    {
                        TaskDialog.Show("错误报告", $"输入参数的值不合法，参数 {parameterSet.Name} 的值 {parameterSet.Value}");
                    }
                }
                transaction.Commit();
            }
        }
    }
}
