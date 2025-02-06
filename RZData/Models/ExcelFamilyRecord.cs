using System.Collections.Generic;

public class ExcelFamilyRecord
{
    public string FamilyCategory { get; set; }
    public string FamilyName { get; set; }
    public string ExtendName { get; set; }
    public Dictionary<string, string> RequiredProperties { get; set; }
    /// <summary>
    /// Ԫ�ط�������
    /// </summary>
    public string ElementName { get; set; }

    public ExcelFamilyRecord()
    {
        RequiredProperties = new Dictionary<string, string>();
    }
}
