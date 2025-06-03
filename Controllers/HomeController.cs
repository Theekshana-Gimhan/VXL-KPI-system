using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace YourProjectName.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Temporary endpoint to reassign roles (run once, then remove)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ReassignRoles()
        {
            // Example: Reassign all users with old roles to new roles
            var users = await _userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                // Remove old roles (e.g., "Admissions", "Vasa Consulting")
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Contains("Admissions") || currentRoles.Contains("Vasa Consulting"))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    // Assign new role based on some logic
                    string newRole = "Staff"; // Default to Staff
                    if (user.Email == "admin@example.com") // Example condition
                    {
                        newRole = "Admin";
                    }
                    else if (user.Email.EndsWith("@manager.com")) // Example condition
                    {
                        newRole = "Manager";
                    }

                    await _userManager.AddToRoleAsync(user, newRole);
                }
            }

            return Content("Roles reassigned successfully!");
        }
    }
}