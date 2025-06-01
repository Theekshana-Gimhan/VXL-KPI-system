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
            startDate ??= DateTime.Today.AddDays(-30); // Last 30 days
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

            var model = new DashboardViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                SelectedDepartment = department ?? "All",
                Departments = departments,
                AdmissionsKPIs = admissionsKPIs,
                VasaConsultingKPIs = vasaConsultingKPIs
            };

            Console.WriteLine($"Loaded Dashboard: {admissionsKPIs.Count} Admissions KPIs, {vasaConsultingKPIs.Count} Vasa Consulting KPIs");
            return View(model);
        }
    }
}