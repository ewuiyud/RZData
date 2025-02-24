using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModels;

namespace Views
{
    /// <summary>
    /// EmbeddedCarbonCalculation.xaml 的交互逻辑
    /// </summary>
    public partial class EmbeddedCarbonCalculationView : Window
    {
        public EmbeddedCarbonCalculationView(UIDocument uIDocument)
        {
            InitializeComponent();
            this.DataContext = new EmbeddedCarbonCalcutionViewModel();
        }
    }
}
