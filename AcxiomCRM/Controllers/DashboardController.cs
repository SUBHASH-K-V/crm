using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(IDashboardService dashboardService, UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            var role = roles.FirstOrDefault() ?? "SalesExecutive";
            var userId = (role == "Admin" || role == "Manager") ? null : user!.Id;
            var vm = await _dashboardService.GetDashboardDataAsync(userId, role);
            ViewBag.UserRole = role;
            ViewBag.UserName = user?.FullName ?? user?.UserName;
            return View(vm);
        }
    }
}

