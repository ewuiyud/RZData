using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RZData
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // ����һ���µ�Ribbonѡ�
            string tabName = "�ס����";
            application.CreateRibbonTab(tabName);

            // ����һ���µ�Ribbon���
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "���ݲ���");

            // ��Ӱ�ť�����
            AddButton(panel, "���ݼ���", "DataValidation", "���ݼ��鹦��", "RZData.Commands.RevitDataCheckCommand", "Resources/data_check.png");
            AddButton(panel, "����¼��", "DataEntry", "����¼�빦��", "Namespace.ClassName2");
            AddButton(panel, "���ݻ���", "DataSummary", "���ݻ��ܹ���", "Namespace.ClassName3");
            AddButton(panel, "����ɸѡ", "DataFilter", "����ɸѡ����", "Namespace.ClassName4");

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void AddButton(RibbonPanel panel, string buttonText, string buttonName, string toolTip, string className)
        {
            PushButtonData buttonData = new PushButtonData(buttonName, buttonText, Assembly.GetExecutingAssembly().Location, className)
            {
                ToolTip = toolTip
            };
            panel.AddItem(buttonData);
        }
        private void AddButton(RibbonPanel panel, string buttonText, string buttonName, string toolTip, string className, string imagePath)
        {
            PushButtonData buttonData = new PushButtonData(buttonName, buttonText, Assembly.GetExecutingAssembly().Location, className)
            {
                ToolTip = toolTip,
                LargeImage = new BitmapImage(new Uri($"pack://application:,,,/RZData;Component/{imagePath}"))
            };
            panel.AddItem(buttonData);
        }
    }
}