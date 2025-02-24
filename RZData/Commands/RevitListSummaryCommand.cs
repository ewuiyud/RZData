﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Views;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RZData.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class RevitListSummaryCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            RevitListSummaryView view = new RevitListSummaryView(uiDocument);
            RevitService.SetWindowTop(view);
            view.Show();
            return Result.Succeeded;
        }
    }
}
