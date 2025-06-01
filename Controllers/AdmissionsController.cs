using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VXL_KPI_system.Data;
using VXL_KPI_system.Data; // Replace with your project namespace
using VXL_KPI_system.Models; // Replace with your project namespace

namespace YourProjectName.Controllers
{
    [Authorize(Roles = "Admin, Counselor")]
    public class AdmissionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdmissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var counselors = _context.Counselors
                .Where(c => c.Department.Name == "Admissions")
                .ToList();

            var model = new AdmissionsDataEntryViewModel
            {
                Date = DateTime.Today,
                CounselorKPIs = counselors.Select(c => new CounselorKPI
                {
                    CounselorID = c.CounselorID,
                    CounselorName = c.Name,
                    Applications = 0,
                    Consultations = 0
                }).ToList()
            };

            if (!model.CounselorKPIs.Any())
            {
                Console.WriteLine("No counselors found for Admissions department!");
            }
            else
            {
                Console.WriteLine($"Loaded {model.CounselorKPIs.Count} counselors for Admissions form");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AdmissionsDataEntryViewModel model)
        {
            Console.WriteLine("Received POST request for Admissions/Create");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid. Errors: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            try
            {
                Console.WriteLine("Starting to process Admissions KPI data...");
                var admissions = _context.Departments.FirstOrDefault(d => d.Name == "Admissions");
                if (admissions == null)
                {
                    Console.WriteLine("Admissions department not found in the database!");
                    return View(model);
                }

                int entriesAdded = 0;
                foreach (var kpi in model.CounselorKPIs)
                {
                    Console.WriteLine($"Processing CounselorID: {kpi.CounselorID}, Applications: {kpi.Applications}, Consultations: {kpi.Consultations}");
                    if (kpi.Applications > 0)
                    {
                        _context.KPIEntries.Add(new KPIEntry
                        {
                            DepartmentID = admissions.DepartmentID,
                            CounselorID = kpi.CounselorID,
                            Date = model.Date,
                            KPItype = "Applications",
                            Value = kpi.Applications
                        });
                        entriesAdded++;
                        Console.WriteLine($"Added Applications entry: {kpi.Applications}");
                    }
                    if (kpi.Consultations > 0)
                    {
                        _context.KPIEntries.Add(new KPIEntry
                        {
                            DepartmentID = admissions.DepartmentID,
                            CounselorID = kpi.CounselorID,
                            Date = model.Date,
                            KPItype = "Consultations",
                            Value = kpi.Consultations
                        });
                        entriesAdded++;
                        Console.WriteLine($"Added Consultations entry: {kpi.Consultations}");
                    }
                }

                if (entriesAdded == 0)
                {
                    Console.WriteLine("No KPI entries were added (all values might be 0)");
                    return View(model);
                }

                _context.SaveChanges();
                Console.WriteLine($"Successfully saved {entriesAdded} KPI entries, redirecting to Home/Index");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving data: " + ex.Message);
                return View(model);
            }
        }
    }
}