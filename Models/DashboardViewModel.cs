public class DashboardViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string SelectedDepartment { get; set; }
    public List<string> Departments { get; set; }
    public List<KPISummary> AdmissionsKPIs { get; set; }
    public List<KPISummary> VasaConsultingKPIs { get; set; }

    // Detailed Metrics
    public int TotalAdmissionsKPIs { get; set; }
    public int TotalVasaConsultingKPIs { get; set; }
    public Dictionary<string, double> AverageKPIsPerCounselor { get; set; } // CounselorName -> Average KPI value

    // Chart Data
    public List<ChartData> AdmissionsChartData { get; set; } // For Admissions KPI trends
    public List<ChartData> VasaConsultingChartData { get; set; } // For Vasa Consulting KPI trends
}

public class KPISummary
{
    public string KPItype { get; set; }
    public string CounselorName { get; set; }
    public int TotalValue { get; set; }
    public DateTime Date { get; set; }
}

public class ChartData
{
    public string Label { get; set; } // e.g., "2025-06-01 Applications"
    public int Value { get; set; }
    public string KPItype { get; set; }
}