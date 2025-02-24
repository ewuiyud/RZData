using CommunityToolkit.Mvvm.Input;
using Models;
using RZData.Services;
using RZData.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace ViewModels
{
    class EmbeddedCarbonCalcutionViewModel : BaseViewModel
    {
        public ICommand ExportCommand { get; }
        public EmbeddedCarbonCalcutionViewModel()
        {
            string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(dllPath, "Resources", "Templates", "TempExcel.xlsx");
            EmbeddedCarbonCalcutionModels = ExcelDataService.ReadEmbeddedCarbonCalcutionExcel(filePath);
            TotalCarbonEmissions = EmbeddedCarbonCalcutionModels.ToList().Sum(a => a.CarbonEmission + a.TransportationCarbonEmission);
            ProductionCarbonEmissions = EmbeddedCarbonCalcutionModels.ToList().Sum(a => a.CarbonEmission);
            TransportationCarbonEmissions = EmbeddedCarbonCalcutionModels.ToList().Sum(a => a.TransportationCarbonEmission);
            ExportCommand = new RelayCommand(Export);
        }

        private void Export()
        {
            string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(dllPath, "Resources", "Templates", "TempExcel.xlsx");
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.Copy(filePath, saveFileDialog.FileName, true);
            }
        }

        private ObservableCollection<EmbeddedCarbonCalcutionModel> embeddedCarbonCalcutionModels;
        public ObservableCollection<EmbeddedCarbonCalcutionModel> EmbeddedCarbonCalcutionModels { get => embeddedCarbonCalcutionModels; set => SetProperty(ref embeddedCarbonCalcutionModels, value); }

        private decimal totalCarbonEmissions;
        public decimal TotalCarbonEmissions { get => totalCarbonEmissions; set => SetProperty(ref totalCarbonEmissions, value); }

        private decimal productionCarbonEmissions;
        public decimal ProductionCarbonEmissions { get => productionCarbonEmissions; set => SetProperty(ref productionCarbonEmissions, value); }

        private decimal transportationnCarbonEmissions;
        public decimal TransportationCarbonEmissions { get => transportationnCarbonEmissions; set => SetProperty(ref transportationnCarbonEmissions, value); }

    }
}
