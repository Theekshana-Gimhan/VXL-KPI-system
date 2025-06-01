public class DashboardViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string SelectedDepartment { get; set; }
    public List<string> Departments { get; set; }
    public List<KPISummary> AdmissionsKPIs { get; set; }
    public List<KPISummary> VasaConsultingKPIs { get; set; }
}

public class KPISummary
{
    public string KPItype { get; set; }
    public string CounselorName { get; set; } // Null for department-wide KPIs like Enquiries
    public int TotalValue { get; set; }
    public DateTime Date { get; set; }
}