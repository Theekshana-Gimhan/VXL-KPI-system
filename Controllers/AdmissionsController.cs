using Microsoft.AspNetCore.Mvc;
using VXL_KPI_system.Data;

public class AdmissionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdmissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        var admissions = _context.Departments.FirstOrDefault(d => d.Name == "Admissions");
        var counselors = _context.Counselors.Where(c => c.DepartmentID == admissions.DepartmentID).ToList();
        var viewModel = new AdmissionsDataEntryViewModel
        {
            Date = DateTime.Today,
            CounselorKPIs = counselors.Select(c => new CounselorKPI
            {
                CounselorID = c.CounselorID,
                CounselorName = c.Name
            }).ToList()
        };
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Create(AdmissionsDataEntryViewModel model)
    {
        if (ModelState.IsValid)
        {
            var admissions = _context.Departments.FirstOrDefault(d => d.Name == "Admissions");
            foreach (var kpi in model.CounselorKPIs)
            {
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
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }
}