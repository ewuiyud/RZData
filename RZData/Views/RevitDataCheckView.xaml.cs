using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Models;
using RZData.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace RZData.Views
{
    /// <summary>
    /// RevitDataCheckView.xaml 的交互逻辑
    /// </summary>
    public partial class RevitDataCheckView : Window
    {
        public RevitDataCheckView(UIDocument uiDocument)
        {
            InitializeComponent();
            DataContext = ViewModelLocator.Instance(uiDocument).RevitDataCheckViewModel;
            Loaded += (s, e) =>
            {
                var textBox = this.FindName("SearchTextBox") as System.Windows.Controls.TextBox;
                if (textBox != null && string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = "请输入关键词搜索";
                    textBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
            };
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var viewModel = DataContext as RevitDataCheckViewModel;
                if (e.NewValue is FamilyExtend familyExtend)
                {
                    viewModel.SelectedItem = familyExtend;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var viewModel = DataContext as RevitDataCheckViewModel;
                viewModel.DoubleClickAndPickObjects((sender as TreeView).SelectedValue);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var viewModel = DataContext as RevitDataCheckViewModel;
                if (viewModel.SearchKeyword != null && viewModel.SearchKeyword != "请输入关键词搜索")
                    viewModel.SearchCommand.Execute(null);
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
                if (textBox.Text == "请输入关键词搜索")
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
                    textBox.Text = "请输入关键词搜索";
                    textBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
    }
}
