using System.ComponentModel.DataAnnotations;

public class Department
{
    [Key]
    public int DepartmentID { get; set; }
    public string Name { get; set; }  // e.g., "Admissions", "Vasa Consulting"
}