using Autodesk.Revit.UI;
using RZData.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RZData.Views
{
    /// <summary>
    /// RevitListSummaryView.xaml 的交互逻辑
    /// </summary>
    public partial class RevitListSummaryView : Window
    {
        public RevitListSummaryView(UIDocument uIDocument)
        {
            InitializeComponent();
            var revitListSummaryViewModel = ViewModelLocator.Instance(uIDocument).RevitListSummaryViewModel;
            revitListSummaryViewModel.GetMaterialListFromDataElement();
            DataContext = revitListSummaryViewModel;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as RevitListSummaryViewModel;
            viewModel.GetAssemblyList();
        }
    }
}
