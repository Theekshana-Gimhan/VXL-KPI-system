using System.ComponentModel.DataAnnotations;

public class Target
{
    [Key]
    public int TargetID { get; set; }
    public int DepartmentID { get; set; }
    public Department Department { get; set; }
    public string KPItype { get; set; }
    public string Period { get; set; }  // e.g., "day", "week", "month"
    public int TargetValue { get; set; }
}