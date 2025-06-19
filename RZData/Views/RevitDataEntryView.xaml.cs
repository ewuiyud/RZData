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
        //private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    try
        //    {
        //        var viewModel = DataContext as RevitDataEntryViewModel;
        //        if (e.NewValue is ViewModels.FamilyExtendViewModel familyExtend)
        //        {
        //            viewModel.SelectedItem = familyExtend;
        //        }
        //        else if (e.NewValue is ViewModels.FamilyViewModel family)
        //        {
        //            viewModel.SelectedItem = family;
        //        }
        //        else if (e.NewValue is ViewModels.ElementInstanceViewModel elementInstance)
        //        {
        //            viewModel.SelectedItem = elementInstance;
        //        }
        //        viewModel.PickObjectsCommand.Execute(null);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        TaskDialog.Show("错误信息", ex.Message);
        //    }
        //}

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
