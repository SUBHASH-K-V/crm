using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(IReportService reportService, UserManager<ApplicationUser> userManager)
        {
            _reportService = reportService;
            _userManager = userManager;
        }

        private async Task PopulateUserDropdown()
        {
            var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Users = users.Select(u => new SelectListItem { Value = u.Id, Text = u.FullName }).ToList();
        }

        public async Task<IActionResult> CustomerReport(string? status, string? ownerId)
        {
            var data = await _reportService.GetCustomerReportAsync(status, ownerId);
            await PopulateUserDropdown();
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedOwner = ownerId;
            return View(data);
        }

        public async Task<IActionResult> LeadReport(string? source, string? status, string? ownerId)
        {
            var data = await _reportService.GetLeadReportAsync(source, status, ownerId);
            await PopulateUserDropdown();
            ViewBag.SelectedSource = source;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedOwner = ownerId;
            return View(data);
        }

        public async Task<IActionResult> FollowUpReport(string? status, string? assignedTo, DateTime? fromDate, DateTime? toDate)
        {
            var data = await _reportService.GetFollowUpReportAsync(status, assignedTo, fromDate, toDate);
            await PopulateUserDropdown();
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedAssigned = assignedTo;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            return View(data);
        }

        public async Task<IActionResult> OpportunityReport(string? stage, string? status, string? ownerId)
        {
            var data = await _reportService.GetOpportunityReportAsync(stage, status, ownerId);
            await PopulateUserDropdown();
            ViewBag.SelectedStage = stage;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedOwner = ownerId;
            return View(data);
        }

        public async Task<IActionResult> PipelineReport()
        {
            var data = await _reportService.GetPipelineReportAsync();
            return View(data);
        }
    }
}

