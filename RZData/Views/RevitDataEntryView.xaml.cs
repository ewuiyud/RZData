using Autodesk.Revit.UI;
using RZData.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RZData.Views
{
    public partial class RevitDataEntryView : Window
    {
        public RevitDataEntryView(UIDocument uiDocument)
        {
            ViewModelLocator.Instance(uiDocument).Reset();
            InitializeComponent();
            var revitDataEntryViewModel = ViewModelLocator.Instance(uiDocument).RevitDataEntryViewModel;
            DataContext = revitDataEntryViewModel;
        }

        private void MultiSelectTreeView_TreeViewDoubleClick(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as RevitDataEntryViewModel;
            viewModel.SelectedItem = e.OriginalSource;
            viewModel.DoubleClickCommand.Execute(null);
        }

        private void FilterProperty_Drop(object sender, System.EventArgs e)
        {
            var viewModel = DataContext as RevitDataEntryViewModel;
            (sender as System.Windows.Controls.ComboBox).ItemsSource = viewModel.GetFilterPropertyList();
            viewModel.SelectedFilterValue = null;
        }

        private void FilterValue_DropDown(object sender, System.EventArgs e)
        {
            var viewModel = DataContext as RevitDataEntryViewModel;
            (sender as System.Windows.Controls.ComboBox).ItemsSource = viewModel.GetFilterValueList();
        }
    }
}
