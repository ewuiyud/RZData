using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.ViewModels
{
    public class AssemblyViewModel
    {
        /// <summary>
        /// 构件名称
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 所属模型
        /// </summary>
        public string Modelbelonging { get; set; }

        /// <summary>
        /// 构件ID
        /// </summary>
        public string AssemblyID { get; set; }
    }
}
