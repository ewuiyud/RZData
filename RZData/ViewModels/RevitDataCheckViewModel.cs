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
            CheckFamiliesCommand = new RelayCommand(CheckFamilies);
            Elements = new ElementData();
        }

        public ElementData Elements { get; }
        public ICommand LoadDataFromExcelCommand { get; }
        public ICommand CheckFamiliesCommand { get; }

        /// <summary>
        /// ��ȡ����
        /// </summary>
        private void RetrieveData()
        {
            var document = _uiDocument.Document;
            var collector = new FilteredElementCollector(document);
            var elements = collector.WhereElementIsNotElementType().ToElements();

            Elements.Clear();
            foreach (var element in elements)
            {
                if (element.Category?.Name == "���˷���")
                {
                    var i = 1;
                }
            }
            //��ȡ���е�ϵͳ���������
            var list = elements.ToList().FindAll(a => a.Category != null);
            foreach (var element in list)
            {
                var familyInstance = element as FamilyInstance;
                if (familyInstance != null)
                {
                    var familyName = familyInstance.Symbol.Family.Name;
                    //Elements.Add(new ElementData
                    //{
                    //    FamilyName = element.Category?.Name ?? "δ֪���",
                    //    TypeName = familyName,
                    //    ExtendName = element.Name
                    //});
                }
                else
                {
                    // ����ϵͳ�Դ����壬��ȡ����������
                    var elementType = document.GetElement(element.GetTypeId()) as ElementType;
                    //Elements.Add(new ElementData
                    //{
                    //    FamilyName = element.Category?.Name ?? "δ֪���", // ϵͳ��� FamilyName ����ʹ���������
                    //    TypeName = elementType?.FamilyName ?? "δ֪���",
                    //    ExtendName = element.Name
                    //});
                }
            }
        }
        private void CheckFamilies()
        {
            var document = _uiDocument.Document;
            var collector = new FilteredElementCollector(document);
            var families = collector.OfClass(typeof(Autodesk.Revit.DB.Family)).Cast<Autodesk.Revit.DB.Family>();

            Elements.Clear();
            foreach (var family in families)
            {
                foreach (var familySymbolId in family.GetFamilySymbolIds())
                {
                    var familySymbol = document.GetElement(familySymbolId) as FamilySymbol;
                    if (familySymbol != null)
                    {
                        Elements.Add(familySymbol);
                    }
                }
            }
        }

        /// <summary>
        /// ��Excel�ļ���������,�����ģ��
        /// </summary>
        private void LoadDataFromExcel()
        {
            List<ExcelRecord> records = ExcelDataProcessor.LoadDataFromExcel();
            CheckModel(records);
        }

        //��������������ģ��
        private void CheckModel(List<ExcelRecord> records)
        {
            var systemFamilyDictionary = records.FindAll(a => a.ExtendName != "����");
            var loadableFamilyDictionary = records.FindAll(a => a.ExtendName == "����");
            var document = _uiDocument.Document;
            var collector = new FilteredElementCollector(document);
            var families = collector.OfClass(typeof(Autodesk.Revit.DB.Family)).Cast<Autodesk.Revit.DB.Family>();

            Elements.Clear();

        }
    }
}
