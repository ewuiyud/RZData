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

namespace RZData.ViewModels.RevitDataEntryViewModel
{

    public class RevitDataEntryViewModel : BaseViewModel
    {
        private object _selectedItem;
        private string _searchKeyword;
        private ElementViewModel _showElements;
        private Models.FamilyCategory _selectedElement;
        public ICommand SearchCommand { get; }
        public ICommand OKCommand { get; }
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                SetProperty(ref _searchKeyword, value);
                if (_searchKeyword != "请输入关键词搜索")
                {
                    SearchCommand.Execute(null);
                }
            }
        }
        public ElementViewModel ShowElements { get => _showElements; set => SetProperty(ref _showElements, value); }
        public ObservableCollection<FamilyCategory> Families
        {
            get
            {
                var fs = new ObservableCollection<FamilyCategory>();
                fs.Add(new FamilyCategory() { Name = "所有" });
                var elements = AllElements;
                elements.FamilyCategories.ToList().ForEach(a => fs.Add(a));
                return fs;
            }
        }
        public Models.FamilyCategory SelectedElement
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
                        var revitSolidElements = AllElements.RevitSolidElements.ToList().FindAll(a => a.FamilyCategory == _selectedElement.Name);
                        ShowElements = new ElementViewModel(revitSolidElements);
                    }
                }
            }
        }
        public RevitDataEntryViewModel(UIDocument _uiDocument, ObservableCollection<RevitSolidElement> revitSolidElements)
        {
            this.AllElements = new ElementViewModel(revitSolidElements.ToList());
            this.ShowElements = AllElements;
            this.UiDocument = _uiDocument;
            SearchCommand = new RelayCommand(Search);
            OKCommand = new AsyncRelayCommand(OK);
        }

        internal void DoubleClickAndPickObjects(object selectedValue)
        {
            switch (selectedValue)
            {
                case FamilyCategory familyCategory:
                    break;
                case Family family:
                    SelectElementInRevit(family);
                    break;
                case FamilyExtend familyExtend:
                    SelectElementInRevit(familyExtend);
                    break;
                case ElementInstance elementInstance:
                    SelectElementInRevit(elementInstance);
                    break;
                default:
                    break;
            }
        }
        private void SelectElementInRevit(Family family)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var id in family.IDs)
            {

                elementIds.Add(new ElementId(id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(FamilyExtend familyExtend)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var id in familyExtend.IDs)
            {
                elementIds.Add(new ElementId(id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(ElementInstance elementInstance)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            elementIds.Add(new ElementId(elementInstance.ID));
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void Search()
        {
            if (_selectedElement == null)
            {
                var revitSolidElements = AllElements.RevitSolidElements.FindAll(a =>
                    a.FamilyName.Contains(SearchKeyword) || a.FamilyCategory.Contains(SearchKeyword) || a.ExtendName.Contains(SearchKeyword));
                ShowElements = new ElementViewModel(revitSolidElements);
            }
            else
            {
                var revitSolidElements = new List<RevitSolidElement>();
                if (_selectedElement.Name == "所有")
                {
                    revitSolidElements = AllElements.RevitSolidElements;
                }
                else
                {
                    revitSolidElements = AllElements.RevitSolidElements.ToList().FindAll(a => a.FamilyCategory == _selectedElement.Name);
                }
                revitSolidElements = revitSolidElements.FindAll(a =>
                    a.FamilyName.Contains(SearchKeyword) || a.FamilyCategory.Contains(SearchKeyword) || a.ExtendName.Contains(SearchKeyword));
                ShowElements = new ElementViewModel(revitSolidElements);
            }
        }
        private async Task OK()
        {
            try
            {
                if (SelectedItem is FamilyExtend familyExtend)
                {
                    foreach (var parameter in familyExtend.Parameters)
                    {
                        if (!parameter.Value.StartsWith("["))
                        {
                            await CustomHandler.Run(a =>
                            {
                                SetParameter(a.ActiveUIDocument, parameter, familyExtend.IDs);
                            });
                        }
                    }
                    //familyExtend.ReloadParameter(UiDocument.Document);
                }
                else if (SelectedItem is ElementInstance elementInstance)
                {
                    foreach (var parameter in elementInstance.Parameters)
                    {
                        await CustomHandler.Run(a =>
                        {
                            SetParameter(a.ActiveUIDocument, parameter, elementInstance.ID);
                        });
                    }
                    //elementInstance.FamilyExtend.ReloadParameter(UiDocument.Document);
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void SetParameter(UIDocument uIDocument, Models.Parameter parameter, int elementId)
        {
            using (Transaction transaction = new Transaction(uIDocument.Document, "SetParameter"))
            {
                var element = uIDocument.Document.GetElement(new ElementId(elementId));
                transaction.Start();
                if (parameter.ValueType == "实例参数")
                {
                    var p = element.LookupParameter(parameter.Name);
                    if (!p.IsReadOnly && !p.Set(parameter.Value))
                    {
                        TaskDialog.Show("错误报告", $"输入参数的值不合法，参数 {parameter.Name} 的值 {parameter.Value}");
                    }
                }
                else if (parameter.ValueType == "类型参数")
                {
                    Element fatherElement = UiDocument.Document.GetElement(element.LookupParameter("族与类型")?.AsElementId());
                    var p = fatherElement.LookupParameter(parameter.Name);
                    if (!p.IsReadOnly && !p.Set(parameter.Value))
                    {
                        TaskDialog.Show("错误报告", $"输入参数的值不合法，参数 {parameter.Name} 的值 {parameter.Value}");
                    }
                }
                transaction.Commit();
            }
        }

        private void SetParameter(UIDocument uIDocument, Models.ParameterSet parameterSet, List<int> IDs)
        {
            using (Transaction transaction = new Transaction(uIDocument.Document, "SetParameter"))
            {
                transaction.Start();
                if (parameterSet.ValueType == "实例参数")
                {
                    foreach (var id in IDs)
                    {
                        Element element = uIDocument.Document.GetElement(new ElementId(id));
                        var p = element.LookupParameter(parameterSet.Name);
                        if (!p.IsReadOnly && !p.Set(parameterSet.Value))
                        {
                            TaskDialog.Show("错误报告", $"输入参数的值不合法，参数 {parameterSet.Name} 的值 {parameterSet.Value}");
                        }
                    }
                }
                else if (parameterSet.ValueType == "类型参数")
                {
                    Element element = UiDocument.Document.GetElement(
                        UiDocument.Document.GetElement(new ElementId(IDs[0])
                        ).LookupParameter("族与类型")?.AsElementId());
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
