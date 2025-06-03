using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VXL_KPI_system.Data; // Replace with your project namespace
using VXL_KPI_system.Models; // Replace with your project namespace
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using VXL_KPI_system.Data;

namespace VXL_KPI_system.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        static DashboardController()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(DateTime? startDate, DateTime? endDate, string department)
        {
            Console.WriteLine($"Loading Dashboard with StartDate: {startDate}, EndDate: {endDate}, Department: {department}");

            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var departments = _context.Departments.Select(d => d.Name).ToList();
            departments.Insert(0, "All");

            var kpiQuery = _context.KPIEntries
                .Include(k => k.Department)
                .Include(k => k.Counselor)
                .Where(k => k.Date >= startDate && k.Date <= endDate);

            if (!string.IsNullOrEmpty(department) && department != "All")
            {
                kpiQuery = kpiQuery.Where(k => k.Department.Name == department);
            }

            var kpiEntries = kpiQuery.ToList();

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

            int totalAdmissionsKPIs = admissionsKPIs.Sum(k => k.TotalValue);
            int totalVasaConsultingKPIs = vasaConsultingKPIs.Sum(k => k.TotalValue);

            var averageKPIsPerCounselor = kpiEntries
                .Where(k => k.Counselor != null)
                .GroupBy(k => k.Counselor.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(k => (double)k.Value)
                );

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportToPDF(DateTime? startDate, DateTime? endDate, string department)
        {
            Console.WriteLine($"Exporting Dashboard to PDF with StartDate: {startDate}, EndDate: {endDate}, Department: {department}");

            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var kpiQuery = _context.KPIEntries
                .Include(k => k.Department)
                .Include(k => k.Counselor)
                .Where(k => k.Date >= startDate && k.Date <= endDate);

            if (!string.IsNullOrEmpty(department) && department != "All")
            {
                kpiQuery = kpiQuery.Where(k => k.Department.Name == department);
            }

            var kpiEntries = kpiQuery.ToList();

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

            int totalAdmissionsKPIs = admissionsKPIs.Sum(k => k.TotalValue);
            int totalVasaConsultingKPIs = vasaConsultingKPIs.Sum(k => k.TotalValue);

            var averageKPIsPerCounselor = kpiEntries
                .Where(k => k.Counselor != null)
                .GroupBy(k => k.Counselor.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(k => (double)k.Value)
                );

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Header().Column(header =>
                    {
                        header.Item().Text("KPI Dashboard Report").FontSize(20).Bold().AlignCenter();
                        header.Item().Text($"Date Range: {startDate.Value.ToString("yyyy-MM-dd")} to {endDate.Value.ToString("yyyy-MM-dd")}").FontSize(12).AlignCenter();
                        header.Item().Text($"Department: {department ?? "All"}").FontSize(12).AlignCenter();
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Item().Text("Summary Metrics").FontSize(16).Bold();
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Total Admissions KPIs: {totalAdmissionsKPIs}");
                            row.RelativeItem().Text($"Total Vasa Consulting KPIs: {totalVasaConsultingKPIs}");
                        });

                        column.Item().Text("Average KPIs per Counselor").FontSize(16).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200);
                                columns.ConstantColumn(100);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Counselor").FontSize(14);
                                header.Cell().Element(CellStyle).Text("Average KPI Value").FontSize(14);
                            });

                            foreach (var avg in averageKPIsPerCounselor)
                            {
                                table.Cell().Element(CellStyle).Text(avg.Key);
                                table.Cell().Element(CellStyle).Text(avg.Value.ToString("F2"));
                            }
                        });

                        column.Item().Text("Admissions KPIs").FontSize(16).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(50);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Date").FontSize(14);
                                header.Cell().Element(CellStyle).Text("KPI Type").FontSize(14);
                                header.Cell().Element(CellStyle).Text("Counselor").FontSize(14);
                                header.Cell().Element(CellStyle).Text("Value").FontSize(14);
                            });

                            foreach (var kpi in admissionsKPIs)
                            {
                                table.Cell().Element(CellStyle).Text(kpi.Date.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).Text(kpi.KPItype);
                                table.Cell().Element(CellStyle).Text(kpi.CounselorName);
                                table.Cell().Element(CellStyle).Text(kpi.TotalValue.ToString());
                            }
                        });

                        column.Item().Text("Vasa Consulting KPIs").FontSize(16).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(50);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Date").FontSize(14);
                                header.Cell().Element(CellStyle).Text("KPI Type").FontSize(14);
                                header.Cell().Element(CellStyle).Text("Staff").FontSize(14);
                                header.Cell().Element(CellStyle).Text("Value").FontSize(14);
                            });

                            foreach (var kpi in vasaConsultingKPIs)
                            {
                                var staffName = kpi.CounselorName ?? "Department";
                                table.Cell().Element(CellStyle).Text(kpi.Date.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).Text(kpi.KPItype);
                                table.Cell().Element(CellStyle).Text(staffName);
                                table.Cell().Element(CellStyle).Text(kpi.TotalValue.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text("Page {pageNumber} of {totalPages}");
                });
            });

            byte[] pdfBytes = pdf.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"KPI_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.Border(1).Padding(5).AlignCenter();
        }
    }
}