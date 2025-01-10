using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Architecture;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using RZData.Models;
using RZData.Helper;

namespace RZData.ViewModels
{
    public class RevitDataCheckViewModel : ObservableObject
    {
        private readonly UIDocument _uiDocument;

        public RevitDataCheckViewModel(UIDocument uiDocument)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _uiDocument = uiDocument;
            LoadDataFromExcelCommand = new RelayCommand(LoadDataFromExcel);
            Elements = new ElementData();
        }

        public ElementData Elements { get; }
        public ICommand LoadDataFromExcelCommand { get; }

        /// <summary>
        /// 从Excel文件加载数据,并检测模型
        /// </summary>
        private void LoadDataFromExcel()
        {
            List<ExcelRecord> records = ExcelDataProcessor.LoadDataFromExcel();
            CheckModel(records);
        }

        //根据族名来检验模型
        private void CheckModel(List<ExcelRecord> records)
        {
            var systemFamilyDictionary = records.FindAll(a => a.ExtendName != "不填");
            var loadableFamilyDictionary = records.FindAll(a => a.ExtendName == "不填");
            var document = _uiDocument.Document;
            var collector = new FilteredElementCollector(document);
            var elements = collector.WhereElementIsNotElementType();

            Elements.Clear();
            ElementData revitElementData = new ElementData();

            foreach (var element in elements)
            {
                if (records.Exists(a => a.FamilyName == element.GetFamily()))
                {
                    revitElementData.Add(element);
                    Elements.Add(element);
                }
            }
        }
    }
}
