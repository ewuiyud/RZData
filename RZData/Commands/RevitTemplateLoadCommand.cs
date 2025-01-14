using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class RevitTemplateLoadCommand : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            RevitTemplateLoadView templateLoad = new RevitTemplateLoadView(uiDocument);
            templateLoad.Show();
            return Result.Succeeded;
        }
    }
}
