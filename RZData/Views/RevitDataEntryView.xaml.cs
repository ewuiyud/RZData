using Autodesk.Revit.UI;
using RZData.Models;
using RZData.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RZData.Views
{
    //exEvent.Raise();
    /// <summary>
    /// RevitDataEntryView.xaml 的交互逻辑
    /// </summary>
    public partial class RevitDataEntryView : Window
    {
        private const string DefaultSearchText = "请输入关键词搜索";
        public RevitDataEntryView(UIDocument uiDocument)
        {
            InitializeComponent();
            var revitDataEntryViewModel = ViewModelLocator.Instance(uiDocument).RevitDataEntryViewModel;
            ViewModelLocator.Instance(uiDocument).Reset();
            DataContext = revitDataEntryViewModel;
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
                var viewModel = DataContext as RevitDataEntryViewModel;
                if (e.NewValue is FamilyExtend familyExtend)
                {
                    viewModel.SelectedItem = familyExtend;
                }
                else if (e.NewValue is DataInstance dataInstance)
                {
                    viewModel.SelectedItem = dataInstance;
                }
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var viewModel = DataContext as RevitDataEntryViewModel;
                viewModel.DoubleClickAndPickObjects((sender as TreeView).SelectedValue);
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var viewModel = DataContext as RevitDataEntryViewModel;
                if (viewModel.SearchKeyword != null && viewModel.SearchKeyword != DefaultSearchText)
                    viewModel.SearchCommand.Execute(null);
            }
            catch (System.Exception ex)
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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
            {
                TaskDialog.Show("错误信息", ex.Message);
            }
        }
        private void SetDefaultSearchText(System.Windows.Controls.TextBox textBox)
        {
            try
            {
                textBox.Text = DefaultSearchText;
                textBox.Foreground = Brushes.Gray;
            }
            catch (System.Exception e)
            {
                TaskDialog.Show("错误信息", e.Message);
            }
        }
    }
}
