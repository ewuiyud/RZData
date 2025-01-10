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
            // 创建一个新的Ribbon选项卡
            string tabName = "睿住数据";
            application.CreateRibbonTab(tabName);

            // 创建一个新的Ribbon面板
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "数据操作");

            // 添加按钮到面板
            AddButton(panel, "数据检验", "DataValidation", "数据检验功能", "RZData.Commands.RevitDataCheckCommand", "Resources/data_check.png");
            AddButton(panel, "数据录入", "DataEntry", "数据录入功能", "Namespace.ClassName2");
            AddButton(panel, "数据汇总", "DataSummary", "数据汇总功能", "Namespace.ClassName3");
            AddButton(panel, "数据筛选", "DataFilter", "数据筛选功能", "Namespace.ClassName4");

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