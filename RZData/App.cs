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
            // 创建一个新的Ribbon选项卡
            string tabName = "睿住数据";
            application.CreateRibbonTab(tabName);

            // 创建一个新的Ribbon面板
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "数据操作");

            // 添加按钮到面板
            AddButton(panel, "模板载入", "Template Load", "模板载入功能", "RZData.Commands.RevitTemplateLoadCommand", "Resources/template_load.png");
            AddButton(panel, "数据检验", "DataValidation", "数据检验功能", "RZData.Commands.RevitDataCheckCommand", "Resources/data_validation.png");
            AddButton(panel, "数据录入", "DataEntry", "数据录入功能", "RZData.Commands.RevitDataEntryCommand", "Resources/data_Entry.png");
            AddButton(panel, "清单计算", "ListSummary", "清单计算功能", "RZData.Commands.RevitListSummaryCommand", "Resources/list_summary.png");
            //AddButton(panel, "隐含碳计算", "EmbeddedCarbonCalculation", "计算建材生成及运输过程中的隐含碳", "RZData.Commands.RevitEmbeddedCarbonCalculationCommand", "Resources/embedded_carbon_calculation.png");
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