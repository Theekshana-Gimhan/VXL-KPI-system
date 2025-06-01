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

        // GET: VasaConsulting/Create
        public IActionResult Create()
        {
            var vasaConsulting = _context.Departments.FirstOrDefault(d => d.Name == "Vasa Consulting");
            var counselors = _context.Counselors.Where(c => c.DepartmentID == vasaConsulting.DepartmentID).ToList();
            var viewModel = new VasaConsultingDataEntryViewModel
            {
                Date = DateTime.Today,
                Enquiries = 0, // Default value, user will update
                StaffKPIs = counselors.Select(c => new StaffKPI
                {
                    CounselorID = c.CounselorID,
                    StaffName = c.Name
                }).ToList()
            };
            return View(viewModel);
        }

        // POST: VasaConsulting/Create
        [HttpPost]
        public IActionResult Create(VasaConsultingDataEntryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var vasaConsulting = _context.Departments.FirstOrDefault(d => d.Name == "Vasa Consulting");
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
                }

                // Save Consultations and Conversions per staff
                foreach (var kpi in model.StaffKPIs)
                {
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
                    }
                }
                _context.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
    }
}