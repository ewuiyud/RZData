using Autodesk.Revit.UI;
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
    /// TemplateLoad.xaml 的交互逻辑
    /// </summary>
    public partial class RevitTemplateLoadView : Window
    {
        public RevitTemplateLoadView(UIDocument uiDocument)
        {
            InitializeComponent();
            DateTime expirationDate = new DateTime(2025, 3, 31);
            if (DateTime.Now > expirationDate)
            {
                MessageBox.Show("当前版本已过期，请更新到最新版本。", "版本更新", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            var revitTemplateLoadViewModel = ViewModelLocator.Instance(uiDocument).RevitTemplateLoadViewModel;
            DataContext = revitTemplateLoadViewModel;
            if (revitTemplateLoadViewModel != null)
            {
                revitTemplateLoadViewModel.CloseAction = new Action(this.Close);
            }
        }
    }
}
