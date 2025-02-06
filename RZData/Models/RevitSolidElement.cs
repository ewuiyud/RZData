using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    public class RevitSolidElement
    {
        public string FamilyCategory { get; set; }
        public string Family { get; set; }
        public string ExtendProperty { get; set; }
        public int ID { get; set; }
    }
}
