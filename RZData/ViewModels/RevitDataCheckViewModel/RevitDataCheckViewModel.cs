using CommunityToolkit.Mvvm.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using RZData.Models;
using System;
using RZData.Services;
using RZData.Extensions;
using WebService;
using RZData.Tools;
using RZData.ExternalEventHandlers;
using Newtonsoft.Json;
using System.Web.UI.WebControls;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class RevitDataCheckViewModel : BaseViewModel
    {
        private string _searchKeyword;

        private ElementViewModel _showElements;
        private object _selectedItem;
        private string _matchResult;
        private bool _showChangeNameBtn;
        private string _matchName;
        private ObservableCollection<ParameterSetVM> _showedParameters;
        public ObservableCollection<ParameterSetVM> ShowedParameters { get => _showedParameters; set => SetProperty(ref _showedParameters, value); }
        public string MatchName { get => _matchName; set => SetProperty(ref _matchName, value); }
        public bool ShowChangeNameBtn { get => _showChangeNameBtn; set => SetProperty(ref _showChangeNameBtn, value); }
        public string MatchResult { get => _matchResult; set => SetProperty(ref _matchResult, value); }

        public RevitDataCheckViewModel(UIDocument uiDocument, ObservableCollection<RevitSolidElement> AllSolidElements)
        {
            UiDocument = uiDocument;
            AllElements = new ElementViewModel(AllSolidElements.ToList());
            FamilyNameCheckElements = new ElementViewModel(AllSolidElements.ToList().FindAll(a => a.IsNameCorrect == false).ToList());
            ParametersCheckElements = new ElementViewModel(AllSolidElements.ToList().FindAll(a => a.IsNameCorrect == true && a.IsPropertiesCorrect == false).ToList());
            ShowParametersCheckElements = ParametersCheckElements;
            //commands
            SearchCommand = new RelayCommand(Search);
            ParameterExportCommand = new RelayCommand(ParameterExport);
            FamilyExportCommand = new RelayCommand(FamilyExport);
            PickObjectsCommand = new RelayCommand(PickObjects);
            AIMatchCommand = new RelayCommand(AIMatch);
            SelectedItemChangedCommand = new RelayCommand(SelectedItemChanged);
            ChangeFamilyNameCommand = new AsyncRelayCommand(ChangeFamilyName);
        }

        private void SelectedItemChanged()
        {
            ShowedParameters = new ObservableCollection<ParameterSetVM>();
            List<ElementInstanceViewModel> elements = new List<ElementInstanceViewModel>();
            if (SelectedItem is FamilyViewModel family)
            {
                if (family.Name.StartsWith("MIC"))
                {
                    elements = family.GetAllElementInstanceViewModels();
                }
            }
            else if (SelectedItem is FamilyExtendViewModel familyExtend) elements = familyExtend.GetAllElementInstanceViewModels();
            var eleParMin = elements.OrderBy(a => a.Parameters.Count).FirstOrDefault();
            if (eleParMin is null)
            {
                return;
            }
            foreach (var item in eleParMin.Parameters)
            {
                if (elements.All(a => a.Parameters.Exists(b => b.Name == item.Name)))
                {
                    var tempParameterSet = new ParameterSetVM(item);
                    elements.ForEach(a =>
                    {
                        var parameter = a.Parameters.FirstOrDefault(b => b.Name == item.Name);
                        if (parameter != null)
                        {
                            if (!tempParameterSet.Parameters.Contains(parameter))
                            {
                                tempParameterSet.Parameters.Add(parameter);
                            }
                        }
                    });
                    ShowedParameters.Add(tempParameterSet);
                }
            }
        }

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
        public object SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public ElementViewModel ShowParametersCheckElements { get => _showElements; set => SetProperty(ref _showElements, value); }
        public ICommand SearchCommand { get; }
        public ICommand ParameterExportCommand { get; }
        public ICommand FamilyExportCommand { get; }
        public ICommand PickObjectsCommand { get; }
        public ICommand AIMatchCommand { get; }
        public ICommand SelectedItemChangedCommand { get; }
        public AsyncRelayCommand ChangeFamilyNameCommand { get; }

        public void AIMatchReset()
        {
            ShowChangeNameBtn = false;
            MatchName = "";
            MatchResult = "";
        }
        private async Task ChangeFamilyName()
        {
            await CustomHandler.Run((Action<UIApplication>)(a =>
            {
                using (Transaction transaction = new Transaction(a.ActiveUIDocument.Document, "ChangeFamilyName"))
                {
                    if (SelectedItem is FamilyExtendViewModel familyExtend)
                    {
                        transaction.Start();
                        Element element = UiDocument.Document.GetElement(new ElementId(familyExtend.IDs[0]));
                        var type = UiDocument.Document.GetElement(element.GetTypeId());
                        type.Name = this.MatchName;
                        transaction.Commit();
                    }
                    else if (SelectedItem is FamilyViewModel family)
                    {
                        transaction.Start();
                        FamilyInstance element = UiDocument.Document.GetElement(new ElementId(family.IDs[0])) as FamilyInstance;
                        var familyElement = element.Symbol.Family;
                        try
                        {
                            familyElement.Name = this.MatchName;
                        }
                        catch (Exception e)
                        {
                            if (e.Message == "Name must be unique.\r\nParameter name: name")
                            {
                                TaskDialog.Show("警告", "命名重复，请添加后缀。");
                            }
                        }
                        transaction.Commit();
                    }
                    ViewModelLocator.Instance(UiDocument).Reset();
                }
            }));
        }
        private void AIMatch()
        {
            if (!(SelectedItem is FamilyExtendViewModel) && !(SelectedItem is FamilyViewModel))
            {
                MatchResult = "";
                ShowChangeNameBtn = false;
                return;
            }

            string categoryName = "";
            string familyName = "";
            string extendName = "";
            List<ExcelFamilyModel> matchList = new List<ExcelFamilyModel>();
            if (SelectedItem is FamilyExtendViewModel familyExtend)
            {
                Element element = UiDocument.Document.GetElement(new ElementId(familyExtend.IDs[0]));
                categoryName = element.GetFamilyCategory();
                familyName = element.GetFamilyName();
                extendName = familyExtend.Name;
                matchList = ExcelDataService.ExcelFamilyRecords.FindAll(a => a.FamilyCategory == categoryName && familyName == a.FamilyName);
                if (matchList.Count == 0)
                {
                    MatchResult = "未找到匹配项";
                    ShowChangeNameBtn = false;
                    return;
                }

                string listString = "";
                foreach (var item in matchList)
                {
                    listString += string.Format($"{item.FamilyCategory}-{item.FamilyName}-{item.ExtendName}\n\t");
                }

                string ak = "sk-93b2d8e03fb04919a89aa235923a7fd0";
                var dp = new DeepSeek(ak);
                var userPromt = new[] { $"待分类数据为：{categoryName}-{familyName}-{extendName}\n" +
                $"分类表内容为{listString}" };
                string systemPromt = FileTool.ReadFileContent("RZData.SystemPromt.MatchNameSystemPromt.txt");
                dp.Chat(userPromt, systemPromt);
                if (!string.IsNullOrEmpty(dp.ErrorMessage))
                {
                    MatchResult = string.Format("ErrorMessage:{0}", dp.ErrorMessage);
                    ShowChangeNameBtn = false;
                }
                else
                {
                    MatchResult = dp.ResultJson.choices[0].message.content;
                    try
                    {
                        var resultJson = JsonConvert.DeserializeObject<FamilyNameJson>(MatchResult);
                        MatchName = resultJson.名称;
                        ShowChangeNameBtn = true;
                    }
                    catch
                    {
                        ShowChangeNameBtn = false;
                    }
                }
            }
            else if (SelectedItem is FamilyViewModel family)
            {
                Element element = UiDocument.Document.GetElement(new ElementId(family.IDs[0]));
                if (!(element is FamilyInstance))
                {
                    MatchResult = "";
                    ShowChangeNameBtn = false;
                    return;
                }
                categoryName = element.GetFamilyCategory();
                familyName = element.GetFamilyName();
                extendName = element.GetExtendName();
                matchList = ExcelDataService.ExcelFamilyRecords.FindAll(a => a.FamilyCategory == categoryName);
                if (matchList.Count == 0)
                {
                    MatchResult = "未找到匹配项";
                    ShowChangeNameBtn = false;
                    return;
                }

                string listString = "";
                foreach (var item in matchList)
                {
                    listString += string.Format($"{item.FamilyCategory}-{item.FamilyName}-{item.ExtendName}\n\t");
                }

                string ak = "sk-93b2d8e03fb04919a89aa235923a7fd0";
                var dp = new DeepSeek(ak);
                var userPromt = new[] { $"待分类数据为：{categoryName}-{familyName}-{extendName}\n" +
                $"分类表内容为{listString}" };
                string systemPromt = FileTool.ReadFileContent("RZData.SystemPromt.MatchNameSystemPromtForLoadFamily.txt");
                dp.Chat(userPromt, systemPromt);
                if (!string.IsNullOrEmpty(dp.ErrorMessage))
                {
                    MatchResult = string.Format("ErrorMessage:{0}", dp.ErrorMessage);
                    ShowChangeNameBtn = false;
                }
                else
                {
                    MatchResult = dp.ResultJson.choices[0].message.content;
                    try
                    {
                        var resultJson = JsonConvert.DeserializeObject<FamilyNameJson>(MatchResult);
                        MatchName = resultJson.族;
                        ShowChangeNameBtn = true;
                    }
                    catch
                    {
                        ShowChangeNameBtn = false;
                    }
                }
            }
        }
        internal void PickObjects()
        {
            switch (SelectedItem)
            {
                case FamilyViewModel family:
                    SelectElementInRevit(family);
                    break;
                case FamilyExtendViewModel familyExtend:
                    SelectElementInRevit(familyExtend);
                    break;
                default:
                    break;
            }
        }
        private void SelectElementInRevit(FamilyViewModel family)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var id in family.IDs)
            {

                elementIds.Add(new ElementId(id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        private void SelectElementInRevit(FamilyExtendViewModel familyExtend)
        {
            var uidoc = UiDocument;
            var elementIds = new List<ElementId>();
            foreach (var id in familyExtend.IDs)
            {
                elementIds.Add(new ElementId(id));
            }
            uidoc.Selection.SetElementIds(elementIds);
        }
        //private void SelectElementInRevit(ElementInstanceViewModel elementInstance)
        //{
        //    var uidoc = UiDocument;
        //    var elementIds = new List<ElementId>
        //    {
        //        new ElementId(elementInstance.Name)
        //    };
        //    uidoc.Selection.SetElementIds(elementIds);
        //}
        public void Search()
        {
            try
            {
                if (string.IsNullOrEmpty(SearchKeyword))
                {
                    ShowParametersCheckElements = ParametersCheckElements;
                }
                var revitSolidElements = ParametersCheckElements.RevitSolidElements.FindAll(a =>
                a.FamilyName.Contains(SearchKeyword) || a.FamilyCategory.Contains(SearchKeyword) || a.ExtendName.Contains(SearchKeyword));
                ShowParametersCheckElements = new ElementViewModel(revitSolidElements);
                Console.WriteLine(1);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        private void ParameterExport()
        {
            try
            {
                ExcelDataService.ExportToExcelFromElement(ShowParametersCheckElements, false);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        private void FamilyExport()
        {
            try
            {
                ExcelDataService.ExportToExcelFromElement(FamilyNameCheckElements);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
    }
}
