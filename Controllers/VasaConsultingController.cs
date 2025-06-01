using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VXL_KPI_system.Data;
using VXL_KPI_system.Data; // Replace with your project namespace
using VXL_KPI_system.Models; // Replace with your project namespace

namespace VXL_KPI_system.Controllers
{
    [Authorize(Roles = "Admin, Counselor")]
    public class VasaConsultingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VasaConsultingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var counselors = _context.Counselors
                .Where(c => c.Department.Name == "Vasa Consulting")
                .ToList();

            var model = new VasaConsultingDataEntryViewModel
            {
                Date = DateTime.Today,
                Enquiries = 0,
                StaffKPIs = counselors.Select(c => new StaffKPI
                {
                    CounselorID = c.CounselorID,
                    StaffName = c.Name,
                    Consultations = 0,
                    Conversions = 0
                }).ToList()
            };

            if (!model.StaffKPIs.Any())
            {
                Console.WriteLine("No staff found for Vasa Consulting department!");
            }
            else
            {
                Console.WriteLine($"Loaded {model.StaffKPIs.Count} staff for Vasa Consulting form");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VasaConsultingDataEntryViewModel model)
        {
            Console.WriteLine("Received POST request for VasaConsulting/Create");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid. Errors: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            try
            {
                Console.WriteLine("Starting to process Vasa Consulting KPI data...");
                var vasaConsulting = _context.Departments.FirstOrDefault(d => d.Name == "Vasa Consulting");
                if (vasaConsulting == null)
                {
                    Console.WriteLine("Vasa Consulting department not found in the database!");
                    return View(model);
                }

                int entriesAdded = 0;

                // Save Enquiries (department-wide KPI)
                if (model.Enquiries > 0)
                {
                    _context.KPIEntries.Add(new KPIEntry
                    {
                        DepartmentID = vasaConsulting.DepartmentID,
                        Date = model.Date,
                        KPItype = "Enquiries",
                        Value = model.Enquiries
                    });
                    entriesAdded++;
                    Console.WriteLine($"Added Enquiries entry: {model.Enquiries}");
                }

                // Save Consultations and Conversions per staff
                foreach (var kpi in model.StaffKPIs)
                {
                    Console.WriteLine($"Processing StaffID: {kpi.CounselorID}, Consultations: {kpi.Consultations}, Conversions: {kpi.Conversions}");
                    if (kpi.Consultations > 0)
                    {
                        _context.KPIEntries.Add(new KPIEntry
                        {
                            DepartmentID = vasaConsulting.DepartmentID,
                            CounselorID = kpi.CounselorID,
                            Date = model.Date,
                            KPItype = "Consultations",
                            Value = kpi.Consultations
                        });
                        entriesAdded++;
                        Console.WriteLine($"Added Consultations entry: {kpi.Consultations}");
                    }
                    if (kpi.Conversions > 0)
                    {
                        _context.KPIEntries.Add(new KPIEntry
                        {
                            DepartmentID = vasaConsulting.DepartmentID,
                            CounselorID = kpi.CounselorID,
                            Date = model.Date,
                            KPItype = "Conversions",
                            Value = kpi.Conversions
                        });
                        entriesAdded++;
                        Console.WriteLine($"Added Conversions entry: {kpi.Conversions}");
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