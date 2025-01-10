using System.Collections.Generic;

public class ExcelRecord
{
    public string FamilyName { get; set; }
    public string TypeName { get; set; }
    public string ExtendName { get; set; }
    public List<string> RequiredProperties { get; set; }

    public ExcelRecord()
    {
        RequiredProperties = new List<string>();
    }
}
