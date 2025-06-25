using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Extensions;
using RZData.Models;
using RZData.Services;
using RZData.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RZData.Tools
{
    public static class PropertyMatchTool
    {
        public static ObservableCollection<MaterialViewModel> FillMaterialList(UIDocument uIDocument, List<RevitSolidElement> list, out List<int> noMatchedElementID)
        {
            ObservableCollection<MaterialViewModel> result = new ObservableCollection<MaterialViewModel>();
            noMatchedElementID = new List<int>();
            foreach (var revitSolidElement in list)
            {
                var materialRecord = GetMaterail(uIDocument, revitSolidElement);

                if (materialRecord != null)
                {
                    var m = result.FirstOrDefault(a => a.MaterialName == materialRecord.MaterialName
                    && a.UsageMethod == materialRecord.UsageMethod
                    && a.ProjectFeatures == materialRecord.ProjectFeatures);
                    if (m != null)
                    {
                        m.RevitSolidElements.Add(revitSolidElement);
                        //五个材料量需要累加
                        m.ModelEngineeringQuantity += materialRecord.ModelEngineeringQuantity;
                        m.MaterialQuantity += materialRecord.MaterialQuantity;
                        if (materialRecord.HasProductMaterialLibrary)
                        {
                            m.ProductMaterialLibrary.MaterialQuantity += materialRecord.ProductMaterialLibrary.MaterialQuantity;
                            m.ProductMaterialLibrary.MaterialProcurementQuantity += materialRecord.ProductMaterialLibrary.MaterialProcurementQuantity;
                        }
                        m.MaterialProcurementQuantity += materialRecord.MaterialProcurementQuantity;
                    }
                    else
                    {
                        materialRecord.RevitSolidElements.Add(revitSolidElement);
                        result.Add(materialRecord);
                    }
                }
                else
                {
                    noMatchedElementID.Add(revitSolidElement.ID);
                }
            }
            return result;
        }
        /// <summary>
        /// 新的解释字符串的方法
        ///1. 字符串中《xx》包裹的内容为查询值，可以从定义好的dictionary中查询对应的值；
        ///2. 字符传中((xx))包裹的内容为固定字符串，直接保留；
        ///3. 字符串[[xx]]包裹的内容为Revit中的元素参数；
        ///4. 字符串中%%xx%%包裹的内容为注释，直接忽略掉。
        /// </summary>
        /// <param name="input"></param>
        /// <param name="revitSolidElement"></param>
        /// <returns></returns>
        public static string ExplainString(UIDocument uIDocument, string input, RevitSolidElement revitSolidElement, MaterialViewModel materialViewModel)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string result = input;
            var dictionary = ExcelDataService.ExcelPropertyDic;
            // 1. 处理《xx》格式 - 字典查询
            result = Regex.Replace(result, @"《([^》]+)》", match =>
            {
                string key = match.Groups[1].Value;
                if (!dictionary.ContainsKey(key))
                {
                    return $"未在属性列表中找到对应的属性id{input}";
                }
                var tDCName = dictionary[key];
                if (tDCName == "TDC-元素分类名称")
                {
                    return revitSolidElement.ElementName;
                }
                else
                {
                    if (materialViewModel.HasProductMaterialLibrary)
                    {
                        var p1 = materialViewModel.ProductMaterialLibrary.SpecificationAttributesDetail.First(a => a.Key == tDCName);
                        if (p1.Value != null)
                        {
                            return p1.Value;
                        }
                    }
                    var p = revitSolidElement.Parameters.FirstOrDefault(a => a.TDCName == tDCName);
                    if (p != null)
                    {
                        return p.Value;
                    }
                    else
                    {
                        return $"不存在的属性项：{tDCName}";
                    }
                }
            });

            // 2. 处理((xx))格式 - 固定字符串，去掉括号
            result = Regex.Replace(result, @"\(\(([^)]+)\)\)", match =>
            {
                return match.Groups[1].Value;
            });

            //3.处理[[xx]]格式 - Revit中的元素参数；
            result = Regex.Replace(result, @"\[\[([^)]+)\]\]", match =>
            {
                return GetModelEngineeringQuantityValue(uIDocument, match.Groups[1].Value, revitSolidElement, materialViewModel);
            });

            // 4. 处理%%xx%%格式 - 注释，直接移除
            result = Regex.Replace(result, @"%%[^%]*%%", "");

            return result;
        }

        /// <summary>
        /// 获取模型中的工程量值，如果是数量，每一个元素返回1.
        /// </summary>
        /// <param name="uIDocument"></param>
        /// <param name="valueName"></param>
        /// <param name="revitSolidElement"></param>
        /// <returns></returns>
        static string GetModelEngineeringQuantityValue(UIDocument uIDocument, string valueName, RevitSolidElement revitSolidElement, MaterialViewModel materialViewModel)
        {
            if (valueName == "数量")
            {
                return "1";
            }
            else if (valueName == "材料工程量")
            {
                if (materialViewModel.HasProductMaterialLibrary)
                {
                    return materialViewModel.ProductMaterialLibrary.MaterialQuantity.ToString();
                }
                else
                {
                    return materialViewModel.ModelEngineeringQuantity.ToString();
                }
            }
            else if (valueName == "损耗率")
            {
                return materialViewModel.LossValue.ToString();
            }

            var doc = uIDocument.Document;
            Element element = doc.GetElement(new ElementId(revitSolidElement.ID));
            var result = element.GetElementValue(doc, valueName);
            if (result != null)
            {
                return result;
            }
            return "0";
        }

        /// <summary>
        /// 获取RevitSolidElement对应的材料信息
        /// </summary>
        /// <param name="uIDocument"></param>
        /// <param name="revitSolidElement"></param>
        /// <returns></returns>
        public static MaterialViewModel GetMaterail(UIDocument uIDocument, RevitSolidElement revitSolidElement)
        {
            var record = SortMaterials(revitSolidElement);
            if (record != null)
            {
                var materialViewModel = new MaterialViewModel
                {
                    MaterialName = record.Name,
                    ProductName = record.ProductName,
                };
                //先挂接产品库
                var element = uIDocument.Document.GetElement(new ElementId(revitSolidElement.ID));
                var materialString = element.LookupParameter("关联材料库").GetValue();
                if (!string.IsNullOrEmpty(materialString))
                {
                    var str = materialString.Split('-');
                    materialViewModel.ProductMaterialLibrary = ExcelDataService.ExcelProductMaterialLibraryModels.FirstOrDefault(a => a.Name == str[0] && a.SerialNumber == str[1]);
                }
                //成功挂接产品库的材料清单
                if (materialViewModel.ProductMaterialLibrary != null)
                {
                    materialViewModel.HasProductMaterialLibrary = true;
                    materialViewModel.MaterialName = materialViewModel.ProductMaterialLibrary.Name;
                }
                if (!string.IsNullOrEmpty(record.ID))
                {
                    materialViewModel.ID = record.ID;
                }
                if (!string.IsNullOrEmpty(record.UsageLocation))
                {
                    materialViewModel.UsageMethod = ExplainString(uIDocument, record.UsageLocation, revitSolidElement, materialViewModel);
                }
                materialViewModel.ProjectFeatures = ExplainString(uIDocument, record.ProjectCharacteristics, revitSolidElement, materialViewModel);
                materialViewModel.ProjectFeaturesDetail = ExplainProjectFeatures(materialViewModel.ProjectFeatures);
                //计算模型工程量
                if (!string.IsNullOrEmpty(record.ModelEngineeringQuantity))
                {
                    var mqStr = ExplainString(uIDocument, record.ModelEngineeringQuantity, revitSolidElement, materialViewModel);
                    try
                    {
                        // 使用DataTable的Compute方法计算表达式
                        double mq = Convert.ToDouble(new System.Data.DataTable().Compute(mqStr, null));
                        materialViewModel.ModelEngineeringQuantity = mq;
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("警告", $"表达式计算失败: {mqStr}\t\n错误详情:{ex}");
                    }
                }
                //模型工程量单位
                if (!string.IsNullOrEmpty(record.ModelEngineeringUnit))
                {
                    materialViewModel.ModelEngineeringUnit = record.ModelEngineeringUnit;
                }
                //计算材料工程量（有库）
                if (!string.IsNullOrEmpty(record.MaterialQuantityHasLibrary))
                {
                    var mqStr = ExplainString(uIDocument, record.MaterialQuantityHasLibrary, revitSolidElement, materialViewModel);
                    if (materialViewModel.HasProductMaterialLibrary)
                        try
                        {
                            // 使用DataTable的Compute方法计算表达式
                            double mq = Convert.ToDouble(new System.Data.DataTable().Compute(mqStr, null));
                            materialViewModel.ProductMaterialLibrary.MaterialQuantity = mq;
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("警告", $"表达式计算失败: {mqStr}\t\n错误详情:{ex}");
                        }
                }
                //计算材料工程量（无库）
                if (!string.IsNullOrEmpty(record.MaterialQuantityNoLibrary))
                {
                    var mqStr = ExplainString(uIDocument, record.MaterialQuantityNoLibrary, revitSolidElement, materialViewModel);
                    try
                    {
                        // 使用DataTable的Compute方法计算表达式
                        double mq = Convert.ToDouble(new System.Data.DataTable().Compute(mqStr, null));
                        materialViewModel.MaterialQuantity = mq;
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("警告", $"表达式计算失败: {mqStr}\t\n错误详情:{ex}");
                    }
                }
                //材料工程量单位
                if (!string.IsNullOrEmpty(record.MaterialUnit))
                {
                    materialViewModel.MaterialUnit = record.MaterialUnit;
                }
                //损耗率
                if (!string.IsNullOrEmpty(record.LossValue))
                {
                    var lossStr = ExplainString(uIDocument, record.LossValue, revitSolidElement, materialViewModel);
                    try
                    {
                        // 使用DataTable的Compute方法计算表达式
                        double loss = Convert.ToDouble(new System.Data.DataTable().Compute(lossStr, null));
                        materialViewModel.LossValue = loss;
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("警告", $"表达式计算失败: {lossStr}\t\n错误详情:{ex}");
                    }
                }

                //计算材料采购量（有库）
                if (!string.IsNullOrEmpty(record.MaterialProcurementQuantityHasLibrary))
                {
                    var mqStr = ExplainString(uIDocument, record.MaterialProcurementQuantityHasLibrary, revitSolidElement, materialViewModel);
                    if (materialViewModel.HasProductMaterialLibrary)
                        try
                        {
                            // 使用DataTable的Compute方法计算表达式
                            double mq = Convert.ToDouble(new System.Data.DataTable().Compute(mqStr, null));
                            materialViewModel.ProductMaterialLibrary.MaterialProcurementQuantity = mq;
                        }
                        catch (Exception ex)
                        {
                            //TaskDialog.Show("警告", $"表达式计算失败: {mqStr}\t\n错误详情:{ex}");
                        }
                }
                //计算材料采购量（无库）
                if (!string.IsNullOrEmpty(record.MaterialProcurementQuantityNoLibrary))
                {
                    var mqStr = ExplainString(uIDocument, record.MaterialProcurementQuantityNoLibrary, revitSolidElement, materialViewModel);
                    try
                    {
                        // 使用DataTable的Compute方法计算表达式
                        double mq = Convert.ToDouble(new System.Data.DataTable().Compute(mqStr, null));
                        materialViewModel.MaterialProcurementQuantity = mq;
                    }
                    catch (Exception ex)
                    {
                        //TaskDialog.Show("警告", $"表达式计算失败: {mqStr}\t\n错误详情:{ex}");
                    }
                }
                //材料采购量的单位
                if (!string.IsNullOrEmpty(record.ProcurementUnit))
                {
                    materialViewModel.ProcurementUnit = record.ProcurementUnit;
                }
                //户型
                var roomP = revitSolidElement.Parameters.FirstOrDefault(a => a.Name == ConstString.RoomName);
                if (roomP != null)
                {
                    materialViewModel.Room = roomP.Value;
                }

                return materialViewModel;
            }
            else
            {
                return null;
            }
        }

        static (string, string) ExplainCodeProperty(string input)
        {
            string temp = input;
            if (!temp.Contains('：'))
            {
                return (null, null);
            }
            string prefix = temp.Split('：')[0];
            string suffix = temp.Split('：')[1];
            return (prefix.Substring(2), suffix);
        }

        public static Dictionary<string, string> ExplainProjectFeatures(string input)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(input))
            {
                return result;
            }
            var features = input.Split('\n');
            foreach (var feature in features)
            {
                var temp = ExplainCodeProperty(feature);
                //对于不包含‘：’的字符串，返回结果为（null,null），不做处理
                if (temp.Item1 == null)
                {
                    continue;
                }
                result.Add(temp.Item1, temp.Item2);
            }
            return result;
        }

        /// <summary>
        /// 根据材料规则匹配表找到revitSolidElement对应的一条记录
        /// </summary>
        /// <param name="revitSolidElement"></param>
        /// <returns></returns>
        public static ExcelMaterialBusinessModel SortMaterials(RevitSolidElement revitSolidElement)
        {
            foreach (var excelMaterialBusinessRecord in ExcelDataService.ExcelMaterialBusinessRules)
            {
                //如果三项都为空，则认为是父级分类
                if (string.IsNullOrEmpty(excelMaterialBusinessRecord.ElementName)
                    && string.IsNullOrEmpty(excelMaterialBusinessRecord.ProductName)
                    && string.IsNullOrEmpty(excelMaterialBusinessRecord.SpaceName))
                    continue;
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ElementName))
                {
                    //根据元素分类名称匹配
                    var elementName = revitSolidElement.ElementName;
                    if (elementName == null)
                        continue;
                    if (!IsStringMatchRule(elementName, excelMaterialBusinessRecord.ElementName, MatchedType.元素分类名称))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ProductName))
                {
                    var p = revitSolidElement.Parameters.FirstOrDefault(a => a.TDCName == "TDC-产品分类名称");
                    if (p == null)
                        continue;
                    if (!IsStringMatchRule(p.Value, excelMaterialBusinessRecord.ProductName, MatchedType.产品分类名称))
                        continue;
                }
                if (!string.IsNullOrEmpty(excelMaterialBusinessRecord.ExtendRule))
                {
                    if (!IsDatainstanceMatchExtendRule(revitSolidElement, excelMaterialBusinessRecord.ExtendRule))
                        continue;
                }
                return excelMaterialBusinessRecord;
            }
            return null;
        }
        enum MatchedType
        {
            元素分类名称,
            产品分类名称
        }

        /// <summary>
        /// 判断输入的名称是否符合判断规则
        /// </summary>
        /// <param name="input"></param>
        /// <param name="rule">输入的规则可能为分类名称的父级</param>
        /// <param name="matchedType"></param>
        /// <returns></returns>
        private static bool IsStringMatchRule(string input, string rule, MatchedType matchedType)
        {
            string[] conditions = rule.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string value in conditions)
            {
                //如果输入的值与规则中的值完全相同，则认为匹配成功
                if (input == value)
                {
                    return true;
                }
                //查表，如果规则中的值是分类名称的父级，则需要查找子分类
                switch (matchedType)
                {
                    case MatchedType.元素分类名称:
                        var elementNode = ExcelDataService.ExcelElementCode.FirstOrDefault(a => a.Value == value);
                        if (elementNode != null)
                        {
                            foreach (var item in elementNode.GetAllChirlds())
                            {
                                if (input == item.Value)
                                {
                                    return true;
                                }
                            }
                        }
                        break;
                    case MatchedType.产品分类名称:
                        var productNode = ExcelDataService.ExcelProductCode.FirstOrDefault(a => a.Value == value);
                        if (productNode != null && productNode.Children.Count == 0)
                        {
                            foreach (var item in productNode.GetAllChirlds())
                            {
                                if (input == item.Value)
                                {
                                    return true;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return false;
        }

        private static bool IsDatainstanceMatchExtendRule(RevitSolidElement revitSolidElement, string rule)
        {
            string[] parts = rule.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                int operatorIndex = trimmedPart.IndexOfAny(new[] { '=', '!' });

                if (operatorIndex < 0)
                {
                    continue;
                }

                string propertyName = trimmedPart.Substring(0, operatorIndex).Trim();
                string propertyValue = trimmedPart.Substring(operatorIndex + 2).Trim();
                bool isEqual = trimmedPart[operatorIndex] == '=';
                var p = revitSolidElement.Parameters.FirstOrDefault(a => a.TDCName == propertyName);
                if (p != null)
                {
                    if (isEqual)
                    {
                        if (IsMatch(p.Value, propertyValue))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (!IsMatch(p.Value, propertyValue))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 使用通配符模式匹配输入字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="wildcardPattern"></param>
        /// <returns></returns>
        public static bool IsMatch(string input, string wildcardPattern)
        {
            if (!wildcardPattern.Contains("*") && !wildcardPattern.Contains("?"))
            {
                return string.Equals(input, wildcardPattern);
            }
            string regexPattern = Regex.Escape(wildcardPattern)
             .Replace(@"\*", ".*");
            regexPattern = $"^{regexPattern}$";
            return Regex.IsMatch(input, regexPattern);
        }
    }
}
