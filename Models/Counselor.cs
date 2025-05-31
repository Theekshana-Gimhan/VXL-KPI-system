using System.ComponentModel.DataAnnotations;

public class Counselor
{
    [Key]
    public int CounselorID { get; set; }
    public string Name { get; set; }  // e.g., "D.W.", "M.B."
    public int DepartmentID { get; set; }
    public Department Department { get; set; }
}