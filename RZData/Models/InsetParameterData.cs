using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Models
{
    /// <summary>
    /// 族属性数据模型
    /// </summary>
    public class InsetParameterData
    {
        public string ParameterName { get; set; }
        public List<string> CategoryNames { get; set; }
        public List<string> FamilyNames { get; set; }

        public InsetParameterData()
        {
            CategoryNames = new List<string>();
            FamilyNames = new List<string>();
        }
    }
}
