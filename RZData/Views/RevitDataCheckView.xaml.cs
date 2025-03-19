using Autodesk.Revit.UI;
using RZData.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace RZData.Views
{
    /// <summary>
    /// RevitDataCheckView.xaml 的交互逻辑
    /// </summary>
    public partial class RevitDataCheckView : Window
    {
        private const string DefaultSearchText = "请输入关键词搜索";
        public RevitDataCheckView(UIDocument uiDocument)
        {
            ViewModelLocator.Instance(uiDocument).Reset();
            InitializeComponent();
            DataContext = ViewModelLocator.Instance(uiDocument).RevitDataCheckViewModel;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var viewModel = DataContext as RevitDataCheckViewModel;
                if (e.NewValue is ViewModels.FamilyExtendViewModel familyExtend)
                {
                    viewModel.SelectedItem = familyExtend;
                }
                else if (e.NewValue is ViewModels.FamilyViewModel family)
                {
                    viewModel.SelectedItem = family;
                }
                viewModel.PickObjectsCommand.Execute(null);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            TreeViewItem treeViewItem = FindVisualParent<TreeViewItem>(textBox);
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
            }
        }
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }
    }
}
