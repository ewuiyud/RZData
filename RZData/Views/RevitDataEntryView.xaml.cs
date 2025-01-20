using Autodesk.Revit.UI;
using RZData.Models;
using RZData.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RZData.Views
{
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
            DataContext = revitDataEntryViewModel;
            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var textBox = this.FindName("SearchTextBox") as System.Windows.Controls.TextBox;
            if (textBox != null && string.IsNullOrEmpty(textBox.Text))
            {
                SetDefaultSearchText(textBox);
            }
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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
        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as RevitDataEntryViewModel;
            viewModel.DoubleClickAndPickObjects((sender as TreeView).SelectedValue);
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var viewModel = DataContext as RevitDataEntryViewModel;
            if (viewModel.SearchKeyword != null && viewModel.SearchKeyword != DefaultSearchText)
                viewModel.SearchCommand.Execute(null);
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox.Text == DefaultSearchText)
            {
                textBox.Text = string.Empty;
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                SetDefaultSearchText(textBox);
            }
        }
        private void SetDefaultSearchText(System.Windows.Controls.TextBox textBox)
        {
            textBox.Text = DefaultSearchText;
            textBox.Foreground = Brushes.Gray;
        }
    }
}
