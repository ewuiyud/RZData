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
using System.Windows.Shapes;

namespace Views
{
    /// <summary>
    /// CarbonProjectView.xaml 的交互逻辑
    /// </summary>
    public partial class CarbonProjectView : Window
    {
        UIDocument UIDocument;
        public CarbonProjectView(UIDocument uIDocument)
        {
            InitializeComponent();
            UIDocument = uIDocument;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EmbeddedCarbonCalculationView embeddedCarbonCalculationView = new EmbeddedCarbonCalculationView(UIDocument);
            this.Close();
            embeddedCarbonCalculationView.Show();
        }
    }
}
