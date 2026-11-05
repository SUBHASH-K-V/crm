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
    public class FollowUpController : Controller
    {
        private readonly IFollowUpService _followUpService;
        private readonly ICustomerService _customerService;
        private readonly ILeadService _leadService;
        private readonly IOpportunityService _opportunityService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowUpController(
            IFollowUpService followUpService,
            ICustomerService customerService,
            ILeadService leadService,
            IOpportunityService opportunityService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _followUpService = followUpService;
            _customerService = customerService;
            _leadService = leadService;
            _opportunityService = opportunityService;
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

            var opps = await _opportunityService.GetAllAsync();
            ViewBag.Opportunities = opps.Select(o => new SelectListItem { Value = o.OpportunityId.ToString(), Text = o.OpportunityName }).ToList();

            var users = _userManager.Users.Where(u => u.IsActive).ToList();
            ViewBag.SalesExecutives = users.Select(u => new SelectListItem { Value = u.Id, Text = u.FullName }).ToList();

            ViewBag.FollowUpTypes = new List<SelectListItem>
            {
                new("Call", "Call"),
                new("Meeting", "Meeting"),
                new("Email", "Email"),
                new("Visit", "Visit")
            };

            ViewBag.Statuses = new List<SelectListItem>
            {
                new("Planned", "Planned"),
                new("Completed", "Completed"),
                new("Missed", "Missed"),
                new("Cancelled", "Cancelled")
            };
        }

        public async Task<IActionResult> Index(DateTime? searchDate, string? searchStatus, string? searchAssigned)
        {
            var (userId, role) = await GetUserContextAsync();
            IEnumerable<FollowUp> followUps;
            if (searchDate.HasValue || !string.IsNullOrWhiteSpace(searchStatus) || !string.IsNullOrWhiteSpace(searchAssigned))
            {
                followUps = await _followUpService.SearchAsync(searchDate, searchStatus, searchAssigned, userId, role);
            }
            else
            {
                followUps = await _followUpService.GetAllAsync(userId, role);
            }

            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchStatus = searchStatus;
            ViewBag.SearchAssigned = searchAssigned;
            ViewBag.ViewMode = "All";
            await PopulateDropdowns();
            return View(followUps);
        }

        public async Task<IActionResult> Upcoming()
        {
            var (userId, role) = await GetUserContextAsync();
            var followUps = await _followUpService.GetUpcomingAsync(userId, role);
            ViewBag.ViewMode = "Upcoming";
            await PopulateDropdowns();
            return View("Index", followUps);
        }

        public async Task<IActionResult> Overdue()
        {
            var (userId, role) = await GetUserContextAsync();
            var followUps = await _followUpService.GetOverdueAsync(userId, role);
            ViewBag.ViewMode = "Overdue";
            await PopulateDropdowns();
            return View("Index", followUps);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            var user = await _userManager.GetUserAsync(User);
            return View(new FollowUpViewModel 
            { 
                FollowUpDate = DateTime.Today,
                Status = "Planned",
                FollowUpType = "Call",
                AssignedTo = user?.Id ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FollowUpViewModel model)
        {
            // Business rule: Follow-up date cannot be earlier than today for planned follow-ups
            if (model.FollowUpDate.Date < DateTime.Today && model.Status == "Planned")
            {
                ModelState.AddModelError("FollowUpDate", "Follow-up date cannot be earlier than today.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var followUp = new FollowUp
            {
                CustomerId = model.CustomerId,
                LeadId = model.LeadId,
                OpportunityId = model.OpportunityId,
                FollowUpDate = model.FollowUpDate,
                Subject = model.Subject,
                FollowUpType = model.FollowUpType,
                Status = model.Status,
                Remarks = model.Remarks,
                AssignedTo = !string.IsNullOrEmpty(model.AssignedTo) ? model.AssignedTo : user!.Id
            };

            var created = await _followUpService.CreateAsync(followUp);
            await _auditService.LogAsync(user!.Id, "Create", "FollowUp", created.FollowUpId.ToString(), null, followUp.Subject, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Follow-up scheduled successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var followUp = await _followUpService.GetByIdAsync(id);
            if (followUp == null) return NotFound();
            await PopulateDropdowns();
            var model = new FollowUpViewModel
            {
                FollowUpId = followUp.FollowUpId,
                CustomerId = followUp.CustomerId,
                LeadId = followUp.LeadId,
                OpportunityId = followUp.OpportunityId,
                FollowUpDate = followUp.FollowUpDate,
                Subject = followUp.Subject,
                FollowUpType = followUp.FollowUpType,
                Status = followUp.Status,
                Remarks = followUp.Remarks,
                AssignedTo = followUp.AssignedTo
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FollowUpViewModel model)
        {
            if (id != model.FollowUpId) return BadRequest();

            if (model.Status == "Planned" && model.FollowUpDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("FollowUpDate", "Follow-up date cannot be earlier than today for planned follow-ups.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            var followUp = await _followUpService.GetByIdAsync(id);
            if (followUp == null) return NotFound();
            var oldValue = $"{followUp.Subject} ({followUp.Status})";

            followUp.CustomerId = model.CustomerId;
            followUp.LeadId = model.LeadId;
            followUp.OpportunityId = model.OpportunityId;
            followUp.FollowUpDate = model.FollowUpDate;
            followUp.Subject = model.Subject;
            followUp.FollowUpType = model.FollowUpType;
            followUp.Status = model.Status;
            followUp.Remarks = model.Remarks;
            followUp.AssignedTo = model.AssignedTo;

            await _followUpService.UpdateAsync(followUp);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Update", "FollowUp", id.ToString(), oldValue, $"{model.Subject} ({model.Status})", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Follow-up updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var followUp = await _followUpService.GetByIdAsync(id);
            if (followUp == null) return NotFound();
            var oldStatus = followUp.Status;
            followUp.Status = "Completed";
            await _followUpService.UpdateAsync(followUp);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Update", "FollowUp", id.ToString(), $"Status: {oldStatus}", "Status: Completed", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Follow-up marked as completed.";
            return RedirectToAction(nameof(Index));
        }
    }
}

