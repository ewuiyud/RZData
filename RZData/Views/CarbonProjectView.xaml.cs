using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Views
{
    /// <summary>
    /// CarbonProjectView.xaml 的交互逻辑
    /// </summary>
    public partial class CarbonProjectView : Window
    {
        readonly UIDocument UIDocument;
        private const string DefaultProjectNameText = "请输入项目名称";
        public CarbonProjectView(UIDocument uIDocument)
        {
            InitializeComponent();
            UIDocument = uIDocument;
            SetDefaultProjectNameText();
        }
        private void SetDefaultProjectNameText()
        {
            if (string.IsNullOrEmpty(ProjectName.Text))
            {
                ProjectName.Text = DefaultProjectNameText;
                ProjectName.Foreground = Brushes.Gray;
            }
        }

        private void ProjectName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ProjectName.Text == DefaultProjectNameText)
            {
                ProjectName.Text = string.Empty;
                ProjectName.Foreground = Brushes.Black;
            }
        }

        private void ProjectName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectName.Text))
            {
                SetDefaultProjectNameText();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EmbeddedCarbonCalculationView embeddedCarbonCalculationView = new EmbeddedCarbonCalculationView(UIDocument);
            this.Close();
            embeddedCarbonCalculationView.Show();
        }
    }
}
