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
    public class RevitDataEntryCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            RevitDataEntryView revitDataEntryView = new RevitDataEntryView(uiDocument);
            revitDataEntryView.Show();
            return Result.Succeeded;
        }
    }
}
