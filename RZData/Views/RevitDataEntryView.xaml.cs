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
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var viewModel = DataContext as RevitDataEntryViewModel;
                if (e.NewValue is ViewModels.FamilyExtendViewModel familyExtend)
                {
                    viewModel.SelectedItem = familyExtend;
                }
                else if (e.NewValue is ViewModels.FamilyViewModel family)
                {
                    viewModel.SelectedItem = family;
                }
                else if (e.NewValue is ViewModels.ElementInstanceViewModel elementInstance)
                {
                    viewModel.SelectedItem = elementInstance;
                }
                viewModel.PickObjectsCommand.Execute(null);
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            var source = item.ItemsSource;
            if (item != null)
            {
                if (item.ItemsSource == null|| item.Items.Count == 0)
                {
                    if (item.DataContext is FamilyExtendViewModel familyExtendViewModel)
                    {
                        item.ItemsSource = familyExtendViewModel.ElementInstances;
                    }
                    else if (item.DataContext is FamilyViewModel familyViewModel)
                    {
                        item.ItemsSource = familyViewModel.ElementInstances;
                    }
                }
                else
                {
                    item.IsExpanded = true;
                }
            }
        }
    }
}
