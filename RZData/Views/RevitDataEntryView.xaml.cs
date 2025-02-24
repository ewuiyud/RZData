using Autodesk.Revit.UI;
using RZData.ViewModels;
using System.Windows;
using System.Windows.Controls;
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
            ViewModelLocator.Instance(uiDocument).Reset();
            InitializeComponent();
            var revitDataEntryViewModel = ViewModelLocator.Instance(uiDocument).RevitDataEntryViewModel;
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
