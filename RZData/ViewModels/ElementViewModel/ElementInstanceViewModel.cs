using CommunityToolkit.Mvvm.ComponentModel;
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
            Parameters = new List<Models.Parameter>();
        }
        public int Name { get; set; }
        public List<Models.Parameter> Parameters { get; set; }
    }
}
