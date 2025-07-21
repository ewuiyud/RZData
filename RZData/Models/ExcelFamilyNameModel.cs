using System.Collections.Generic;

/// <summary>
/// 族匹配表
/// </summary>
public class ExcelFamilyNameModel
{
    public string FamilyCategory { get; set; }
    public string FamilyName { get; set; }
    public string ExtendName { get; set; }
    public List<ExcelParameterModel> RequiredProperties { get; set; }
    /// <summary>
    /// 元素分类名称
    /// </summary>
    public string ElementName { get; set; }

    public ExcelFamilyNameModel()
    {
        RequiredProperties = new List<ExcelParameterModel>();
    }
}

/// <summary>
/// 族匹配表中的参数模型
/// </summary>
public class ExcelParameterModel
{
    public string Name { get; set; }
    public string TDCName { get; set; }
    public string ValueEnumString { get; set; }
    public string StandardValue { get; set; }
    public string Unit { get; set; }
    public bool IsShowed { get; set; } = true;
    public string Reference { get; set; }
}
