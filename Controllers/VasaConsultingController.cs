using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VXL_KPI_system.Data; // Replace with your project namespace
using VXL_KPI_system.Models; // Replace with your project namespace
using System;
using System.Threading.Tasks;
using VXL_KPI_system.Data;

namespace VXL_KPI_system.Controllers
{
    [Authorize(Roles = "Staff")]
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KPIEntry model)
        {
            if (ModelState.IsValid)
            {
                model.Department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Vasa Consulting");
                if (model.Department == null)
                {
                    ModelState.AddModelError("", "Vasa Consulting department not found.");
                    return View(model);
                }

                if (model.KPItype != "Enquiries")
                {
                     // Find the counselor by username
                    var counselor = await _context.Counselors.FirstOrDefaultAsync(c => c.Name == User.Identity.Name);
                    if (counselor == null)
                    {
                        ModelState.AddModelError("", "Counselor not found.");
                        return View(model);
                    }
                    model.CounselorID = counselor.CounselorID;

                }
                _context.KPIEntries.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
    }
}