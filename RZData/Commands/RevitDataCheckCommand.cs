using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
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
    public class RevitDataCheckCommand : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            RevitDataCheckView revitDataCheckView = new RevitDataCheckView(uiDocument);
            revitDataCheckView.Show();
            return Result.Succeeded;
        }
    }
}
