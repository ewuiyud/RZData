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
        public RevitListSummaryView(UIDocument uiDocument)
        {
            ViewModelLocator.Instance(uiDocument).Reset();
            InitializeComponent();
            var revitListSummaryViewModel = ViewModelLocator.Instance(uiDocument).RevitListSummaryViewModel;
            revitListSummaryViewModel.GetMaterialListFromDataElement();
            DataContext = revitListSummaryViewModel;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as RevitListSummaryViewModel;
            viewModel.GetAssemblyList();
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var viewModel = DataContext as RevitListSummaryViewModel;
            viewModel.PropertyNameDroped();
        }

        private void ComboBox_DropDownOpened_1(object sender, EventArgs e)
        {
            var viewModel = DataContext as RevitListSummaryViewModel;
            viewModel.PropertyValueDroped();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as RevitListSummaryViewModel;
            viewModel.SelectedPropertyValue = null;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as RevitListSummaryViewModel;
            viewModel.SelectedPropertyValue = null;
            viewModel.DoubleClickAndPickObjects();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
