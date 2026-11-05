using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly IReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogController(IReportService reportService, UserManager<ApplicationUser> userManager)
        {
            _reportService = reportService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? userId, string? action, string? module, DateTime? fromDate, DateTime? toDate)
        {
            var logs = await _reportService.GetAuditReportAsync(userId, action, module, fromDate, toDate);
            var users = await _userManager.Users.ToListAsync();

            ViewBag.Users = users.Select(u => new SelectListItem { Value = u.Id, Text = u.FullName }).ToList();
            ViewBag.SelectedUserId = userId;
            ViewBag.SelectedAction = action;
            ViewBag.SelectedModule = module;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(logs);
        }
    }
}

