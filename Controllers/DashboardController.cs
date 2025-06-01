using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VXL_KPI_system.Data;
using VXL_KPI_system.Data; // Replace with your project namespace
using VXL_KPI_system.Models; // Replace with your project namespace

namespace VXL_KPI_system.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(DateTime? startDate, DateTime? endDate, string department)
        {
            Console.WriteLine($"Loading Dashboard with StartDate: {startDate}, EndDate: {endDate}, Department: {department}");

            // Set default date range if not provided
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            // Get all departments for the filter dropdown
            var departments = _context.Departments.Select(d => d.Name).ToList();
            departments.Insert(0, "All");

            // Base query for KPI entries
            var kpiQuery = _context.KPIEntries
                .Include(k => k.Department)
                .Include(k => k.Counselor)
                .Where(k => k.Date >= startDate && k.Date <= endDate);

            // Apply department filter if specified
            if (!string.IsNullOrEmpty(department) && department != "All")
            {
                kpiQuery = kpiQuery.Where(k => k.Department.Name == department);
            }

            // Fetch all KPI entries
            var kpiEntries = kpiQuery.ToList();

            // Aggregate Admissions KPIs
            var admissionsKPIs = kpiEntries
                .Where(k => k.Department.Name == "Admissions")
                .GroupBy(k => new { k.KPItype, k.CounselorID, k.Date })
                .Select(g => new KPISummary
                {
                    KPItype = g.Key.KPItype,
                    CounselorName = g.First().Counselor?.Name ?? "N/A",
                    TotalValue = g.Sum(k => k.Value),
                    Date = g.Key.Date
                })
                .OrderBy(k => k.Date)
                .ThenBy(k => k.CounselorName)
                .ToList();

            // Aggregate Vasa Consulting KPIs
            var vasaConsultingKPIs = kpiEntries
                .Where(k => k.Department.Name == "Vasa Consulting")
                .GroupBy(k => new { k.KPItype, k.CounselorID, k.Date })
                .Select(g => new KPISummary
                {
                    KPItype = g.Key.KPItype,
                    CounselorName = g.Key.KPItype == "Enquiries" ? null : g.First().Counselor?.Name,
                    TotalValue = g.Sum(k => k.Value),
                    Date = g.Key.Date
                })
                .OrderBy(k => k.Date)
                .ThenBy(k => k.CounselorName ?? "Department")
                .ToList();

            // Compute detailed metrics
            int totalAdmissionsKPIs = admissionsKPIs.Sum(k => k.TotalValue);
            int totalVasaConsultingKPIs = vasaConsultingKPIs.Sum(k => k.TotalValue);

            // Compute average KPIs per counselor
            var averageKPIsPerCounselor = kpiEntries
                .Where(k => k.Counselor != null) // Exclude department-wide KPIs like Enquiries
                .GroupBy(k => k.Counselor.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(k => (double)k.Value)
                );

            // Prepare chart data for Admissions
            var admissionsChartData = admissionsKPIs
                .GroupBy(k => new { k.Date, k.KPItype })
                .Select(g => new ChartData
                {
                    Label = $"{g.Key.Date:yyyy-MM-dd} {g.Key.KPItype}",
                    Value = g.Sum(k => k.TotalValue),
                    KPItype = g.Key.KPItype
                })
                .OrderBy(c => c.Label)
                .ToList();

            // Prepare chart data for Vasa Consulting
            var vasaConsultingChartData = vasaConsultingKPIs
                .GroupBy(k => new { k.Date, k.KPItype })
                .Select(g => new ChartData
                {
                    Label = $"{g.Key.Date:yyyy-MM-dd} {g.Key.KPItype}",
                    Value = g.Sum(k => k.TotalValue),
                    KPItype = g.Key.KPItype
                })
                .OrderBy(c => c.Label)
                .ToList();

            var model = new DashboardViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                SelectedDepartment = department ?? "All",
                Departments = departments,
                AdmissionsKPIs = admissionsKPIs,
                VasaConsultingKPIs = vasaConsultingKPIs,
                TotalAdmissionsKPIs = totalAdmissionsKPIs,
                TotalVasaConsultingKPIs = totalVasaConsultingKPIs,
                AverageKPIsPerCounselor = averageKPIsPerCounselor,
                AdmissionsChartData = admissionsChartData,
                VasaConsultingChartData = vasaConsultingChartData
            };

            Console.WriteLine($"Loaded Dashboard: {admissionsKPIs.Count} Admissions KPIs, {vasaConsultingKPIs.Count} Vasa Consulting KPIs");
            return View(model);
        }
    }
}