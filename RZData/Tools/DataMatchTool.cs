using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RZData.Tools
{
    public class DataMatchTool
    {
        private static readonly Regex VariablePattern = new Regex(@"%%([^%]+)%%");
        // 解析并计算表达式的值
        public static double Evaluate(string formula, Func<string, string> getValue)
        {
            if (string.IsNullOrEmpty(formula))
                throw new ArgumentNullException(nameof(formula), "表达式不能为空");

            // 替换所有占位符为实际值
            string resolvedFormula = ReplacePlaceholders(formula, getValue);

            try
            {
                // 使用DataTable的Compute方法计算表达式
                return Convert.ToDouble(new System.Data.DataTable().Compute(resolvedFormula, null));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"表达式计算失败: {resolvedFormula}", ex);
            }
        }

        // 将占位符替换为实际变量值
        private static string ReplacePlaceholders(string formula, Func<string, string> getValue)
        {
            return VariablePattern.Replace(formula, match =>
            {
                string variableName = match.Groups[1].Value;
                var value = getValue(variableName);
                return value;
            });
        }
    }
}
