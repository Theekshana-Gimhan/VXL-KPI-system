using System.ComponentModel.DataAnnotations;

public class KPIEntry
{
    [Key]
    public int EntryID { get; set; }
    public int DepartmentID { get; set; }
    public Department Department { get; set; }
    public int? CounselorID { get; set; }  // Nullable for department-wide KPIs like Enquiries
    public Counselor Counselor { get; set; }
    public DateTime Date { get; set; }
    public string KPItype { get; set; }  // e.g., "Applications", "Enquiries"
    public int Value { get; set; }
}