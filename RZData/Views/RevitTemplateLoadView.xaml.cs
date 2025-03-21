using Autodesk.Revit.UI;
using RZData.ViewModels;
using System;
using System.Windows;

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
//#if DEBUG
//            string executablePath = Assembly.GetExecutingAssembly().Location;
//            DateTime creationTime = File.GetCreationTime(executablePath);
//            DateTime expirationDate = creationTime/*.AddMonths(1)*/;
//            if (DateTime.Now >= expirationDate)
//            {
//                MessageBox.Show($"当前测试版本于{creationTime.ToShortDateString()}过期，请及时更新。", "版本更新", MessageBoxButton.OK, MessageBoxImage.Warning);
//            }
//#endif
            var revitTemplateLoadViewModel = ViewModelLocator.Instance(uiDocument).RevitTemplateLoadViewModel;
            DataContext = revitTemplateLoadViewModel;
            if (revitTemplateLoadViewModel != null)
            {
                revitTemplateLoadViewModel.CloseAction = new Action(this.Close);
            }
        }
    }
}