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
    public class RevitTemplateLoadCommand : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            RevitTemplateLoadView view = new RevitTemplateLoadView(uiDocument);
            RevitService.SetWindowTop(view);
            view.Show();
            return Result.Succeeded;
        }
    }
}
