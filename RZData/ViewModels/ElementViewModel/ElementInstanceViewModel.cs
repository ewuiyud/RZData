using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using RZData.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class ElementInstanceViewModel : ObservableObject
    {
        public ElementInstanceViewModel()
        {
            Parameters = new List<ParameterVM>();
        }
        public FamilyExtendViewModel Parent { get; set; }
        public int Id { get; set; }
        public List<ParameterVM> Parameters { get; set; }

        private bool isChecked;
        public bool IsChecked { get => isChecked; set => SetProperty(ref isChecked, value); }
    }
}
