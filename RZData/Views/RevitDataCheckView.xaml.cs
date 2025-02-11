using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Models;
using RZData.ViewModels;
using RZData.ViewModels.RevitDataCheckViewModel;
using System;
using System.Drawing;
using System.Threading.Tasks;
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
            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = this.FindName("SearchTextBox") as System.Windows.Controls.TextBox;
                if (textBox != null && string.IsNullOrEmpty(textBox.Text))
                {
                    SetDefaultSearchText(textBox);
                }
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var viewModel = DataContext as RevitDataCheckViewModel;
                if (e.NewValue is ViewModels.FamilyExtend familyExtend)
                {
                    viewModel.SelectedItem = familyExtend;
                }
                else if (e.NewValue is ViewModels.Family family)
                {
                    viewModel.SelectedItem = family;
                }
                viewModel.DoubleClickAndPickObjects(viewModel.SelectedItem);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = sender as System.Windows.Controls.TextBox;
                if (textBox.Text == DefaultSearchText)
                {
                    textBox.Text = string.Empty;
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = sender as System.Windows.Controls.TextBox;
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    SetDefaultSearchText(textBox);
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        private void SetDefaultSearchText(System.Windows.Controls.TextBox textBox)
        {
            try
            {
                textBox.Text = DefaultSearchText;
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
            catch (System.Exception e)
            {
                TaskDialog.Show("错误信息", e.Message);
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
