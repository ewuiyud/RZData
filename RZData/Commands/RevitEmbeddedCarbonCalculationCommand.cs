using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Views;
using Services;
using System;
using System.Collections.Generic;
using System.Text;
using Views;

namespace RZData.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class RevitEmbeddedCarbonCalculationCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            CarbonProjectView view = new CarbonProjectView(uiDocument);
            RevitService.SetWindowTop(view);
            view.Show();
            return Result.Succeeded;
        }
    }
}
