using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class ActivityController : Controller
    {
        private readonly IActivityService _activityService;
        private readonly ICustomerService _customerService;
        private readonly ILeadService _leadService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivityController(
            IActivityService activityService,
            ICustomerService customerService,
            ILeadService leadService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _activityService = activityService;
            _customerService = customerService;
            _leadService = leadService;
            _auditService = auditService;
            _userManager = userManager;
        }

        private async Task<(string userId, string role)> GetUserContextAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            return (user!.Id, roles.FirstOrDefault() ?? "SalesExecutive");
        }

        private async Task PopulateDropdowns()
        {
            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = customers.Select(c => new SelectListItem { Value = c.CustomerId.ToString(), Text = c.CustomerName }).ToList();
            
            var leads = await _leadService.GetAllAsync();
            ViewBag.Leads = leads.Select(l => new SelectListItem { Value = l.LeadId.ToString(), Text = l.LeadName }).ToList();

            var users = _userManager.Users.Where(u => u.IsActive).ToList();
            ViewBag.SalesExecutives = users.Select(u => new SelectListItem { Value = u.Id, Text = u.FullName }).ToList();

            ViewBag.ActivityTypes = new List<SelectListItem>
            {
                new("Call", "Call"),
                new("Meeting", "Meeting"),
                new("Email", "Email"),
                new("Task", "Task")
            };

            ViewBag.Statuses = new List<SelectListItem>
            {
                new("Planned", "Planned"),
                new("Completed", "Completed"),
                new("Cancelled", "Cancelled")
            };
        }

        public async Task<IActionResult> Index(string? searchType, DateTime? searchDate, string? searchStatus, string? searchAssigned)
        {
            var (userId, role) = await GetUserContextAsync();
            IEnumerable<Activity> activities;
            if (!string.IsNullOrWhiteSpace(searchType) || searchDate.HasValue || 
                !string.IsNullOrWhiteSpace(searchStatus) || !string.IsNullOrWhiteSpace(searchAssigned))
            {
                activities = await _activityService.SearchAsync(searchType, searchDate, searchStatus, searchAssigned, userId, role);
            }
            else
            {
                activities = await _activityService.GetAllAsync(userId, role);
            }

            ViewBag.SearchType = searchType;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchStatus = searchStatus;
            ViewBag.SearchAssigned = searchAssigned;
            await PopulateDropdowns();
            return View(activities);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            var user = await _userManager.GetUserAsync(User);
            return View(new ActivityViewModel 
            { 
                ActivityDate = DateTime.Today,
                ActivityType = "Call",
                Status = "Planned",
                AssignedTo = user?.Id ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var activity = new Activity
            {
                ActivityType = model.ActivityType,
                Subject = model.Subject,
                Description = model.Description,
                ActivityDate = model.ActivityDate,
                CustomerId = model.CustomerId,
                LeadId = model.LeadId,
                AssignedTo = !string.IsNullOrEmpty(model.AssignedTo) ? model.AssignedTo : user!.Id,
                Status = model.Status
            };

            var created = await _activityService.CreateAsync(activity);
            await _auditService.LogAsync(user!.Id, "Create", "Activity", created.ActivityId.ToString(), null, activity.Subject, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Activity created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var activity = await _activityService.GetByIdAsync(id);
            if (activity == null) return NotFound();
            await PopulateDropdowns();
            var model = new ActivityViewModel
            {
                ActivityId = activity.ActivityId,
                ActivityType = activity.ActivityType,
                Subject = activity.Subject,
                Description = activity.Description,
                ActivityDate = activity.ActivityDate,
                CustomerId = activity.CustomerId,
                LeadId = activity.LeadId,
                AssignedTo = activity.AssignedTo,
                Status = activity.Status
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActivityViewModel model)
        {
            if (id != model.ActivityId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            var activity = await _activityService.GetByIdAsync(id);
            if (activity == null) return NotFound();
            var oldValue = $"{activity.ActivityType}: {activity.Subject} ({activity.Status})";

            activity.ActivityType = model.ActivityType;
            activity.Subject = model.Subject;
            activity.Description = model.Description;
            activity.ActivityDate = model.ActivityDate;
            activity.CustomerId = model.CustomerId;
            activity.LeadId = model.LeadId;
            activity.AssignedTo = model.AssignedTo;
            activity.Status = model.Status;

            await _activityService.UpdateAsync(activity);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Update", "Activity", id.ToString(), oldValue, $"{model.ActivityType}: {model.Subject} ({model.Status})", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Activity updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var activity = await _activityService.GetByIdAsync(id);
            if (activity == null) return NotFound();
            await _activityService.DeleteAsync(id);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Delete", "Activity", id.ToString(), activity.Subject, "Deleted", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Activity deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

