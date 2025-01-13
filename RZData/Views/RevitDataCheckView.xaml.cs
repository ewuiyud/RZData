using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Models;
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
    /// RevitDataCheckView.xaml 的交互逻辑
    /// </summary>
    public partial class RevitDataCheckView : Window
    {
        public RevitDataCheckView(UIDocument uiDocument)
        {
            InitializeComponent();
            DataContext = new RevitDataCheckViewModel(uiDocument);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var viewModel = DataContext as RevitDataCheckViewModel;
            if (e.NewValue is DataInstance dataInstance)
            {
                viewModel.SelectedItem = dataInstance;
            }
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as RevitDataCheckViewModel;
            viewModel.DoubleClickAndPickObjects((sender as TreeView).SelectedValue);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as RevitDataCheckViewModel;
            viewModel.OptionChanged();
        }
    }
}
