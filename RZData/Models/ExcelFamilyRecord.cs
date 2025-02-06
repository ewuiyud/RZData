using System.Collections.Generic;

public class ExcelFamilyRecord
{
    public string FamilyCategory { get; set; }
    public string FamilyName { get; set; }
    public string ExtendName { get; set; }
    public Dictionary<string, string> RequiredProperties { get; set; }
    /// <summary>
    /// 元素分类名称
    /// </summary>
    public string ElementName { get; set; }

    public ExcelFamilyRecord()
    {
        RequiredProperties = new Dictionary<string, string>();
    }
}
