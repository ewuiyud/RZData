using RZData.Models;
using RZData.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Ribbon.Primitives;

namespace RZData.Tools
{
    public class FilterTool
    {
        public static bool FilterRevitElement(RevitSolidElement revitSolidElement, List<FilterConditionViewModel> filterConditions)
        {
            return filterConditions.All(f => FilterRevitElement(revitSolidElement, f));
        }
        public static bool FilterRevitElement(RevitSolidElement revitSolidElement, FilterConditionViewModel filterCondition)
        {
            // 检查元素是否有参数集合
            if (revitSolidElement.Parameters == null || revitSolidElement.Parameters.Count == 0)
            {
                return false;
            }

            // 获取要筛选的参数
            string propertyName = filterCondition.PropertyName;
            string propertyValue = filterCondition.PropertyValue;
            string logic = filterCondition.Logic;

            // 查找指定名称的参数
            ParameterVM parameter = revitSolidElement.Parameters.FirstOrDefault(p => p.Name == propertyName);
            if (parameter == null || string.IsNullOrEmpty(parameter.Value))
            {
                return false;
            }

            string parameterValue = parameter.Value;

            // 根据不同逻辑执行不同的筛选条件
            switch (logic)
            {
                case "等于":
                    return string.Equals(parameterValue, propertyValue, StringComparison.OrdinalIgnoreCase);

                case "不等于":
                    return !string.Equals(parameterValue, propertyValue, StringComparison.OrdinalIgnoreCase);

                case "大于等于":
                    return CheckAndCompareNumbers(parameterValue, propertyValue, (a, b) => a >= b);

                case "小于等于":
                    return CheckAndCompareNumbers(parameterValue, propertyValue, (a, b) => a <= b);

                case "正则表达式":
                    return CheckAndMatchRegex(parameterValue, propertyValue);

                default:
                    throw new ArgumentException($"不支持的筛选逻辑: {logic}");
            }
        }
        /// <summary>
        /// 检查并比较数字
        /// </summary>
        private static bool CheckAndCompareNumbers(string paramValue, string filterValue, Func<double, double, bool> comparison)
        {
            if (!double.TryParse(paramValue, out double paramNum))
            {
                throw new ArgumentException("参数值必须为数字");
            }

            if (!double.TryParse(filterValue, out double filterNum))
            {
                throw new ArgumentException("筛选条件值必须为数字");
            }

            return comparison(paramNum, filterNum);
        }

        /// <summary>
        /// 检查并匹配正则表达式
        /// </summary>
        private static bool CheckAndMatchRegex(string paramValue, string regexPattern)
        {
            try
            {
                // 尝试编译正则表达式，检查其有效性
                Regex regex = new Regex(regexPattern);
                return regex.IsMatch(paramValue);
            }
            catch 
            {
                throw new ArgumentException("无效的正则表达式模式");
            }
        }
    }
}
