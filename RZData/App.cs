using Autodesk.Revit.UI;
using Prism.DryIoc;
using Prism.Ioc;
using RZData.ExternalEventHandlers;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RZData
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            CustomHandler.Instance = new CustomHandler();
            // ����һ���µ�Ribbonѡ�
            string tabName = "�ס����";
            application.CreateRibbonTab(tabName);

            // ����һ���µ�Ribbon���
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "���ݲ���");

            // ��Ӱ�ť�����
            AddButton(panel, "ģ������", "Template Load", "ģ�����빦��", "RZData.Commands.RevitTemplateLoadCommand", "Resources/template_load.png");
            AddButton(panel, "���ݼ���", "DataValidation", "���ݼ��鹦��", "RZData.Commands.RevitDataCheckCommand", "Resources/data_validation.png");
            AddButton(panel, "����¼��", "DataEntry", "����¼�빦��", "RZData.Commands.RevitDataEntryCommand", "Resources/data_Entry.png");
            AddButton(panel, "�嵥����", "ListSummary", "�嵥���㹦��", "RZData.Commands.RevitListSummaryCommand", "Resources/list_summary.png");
            //AddButton(panel, "����̼����", "EmbeddedCarbonCalculation", "���㽨�����ɼ���������е�����̼", "RZData.Commands.RevitEmbeddedCarbonCalculationCommand", "Resources/embedded_carbon_calculation.png");
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
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