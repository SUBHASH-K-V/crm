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
    public class LeadController : Controller
    {
        private readonly ILeadService _leadService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeadController(ILeadService leadService, IAuditService auditService, UserManager<ApplicationUser> userManager)
        {
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
            var users = _userManager.Users.Where(u => u.IsActive).ToList();
            ViewBag.SalesExecutives = users.Select(u => new SelectListItem { Value = u.Id, Text = u.FullName + " (" + u.Email + ")" }).ToList();
            ViewBag.Statuses = new List<SelectListItem>
            {
                new("New", "New"),
                new("Contacted", "Contacted"),
                new("Qualified", "Qualified"),
                new("Unqualified", "Unqualified"),
                new("Converted", "Converted"),
                new("Lost", "Lost")
            };
            ViewBag.Sources = new List<SelectListItem>
            {
                new("Website", "Website"),
                new("Referral", "Referral"),
                new("Social Media", "Social Media"),
                new("Email Campaign", "Email Campaign"),
                new("Cold Call", "Cold Call"),
                new("Trade Show", "Trade Show"),
                new("Other", "Other")
            };
            ViewBag.Priorities = new List<SelectListItem>
            {
                new("Low", "Low"),
                new("Medium", "Medium"),
                new("High", "High")
            };
        }

        public async Task<IActionResult> Index(string? searchName, string? searchCompany, string? searchStatus, string? searchAssigned)
        {
            var (userId, role) = await GetUserContextAsync();
            IEnumerable<Lead> leads;
            if (!string.IsNullOrWhiteSpace(searchName) || !string.IsNullOrWhiteSpace(searchCompany) || 
                !string.IsNullOrWhiteSpace(searchStatus) || !string.IsNullOrWhiteSpace(searchAssigned))
            {
                leads = await _leadService.SearchAsync(searchName, searchCompany, searchStatus, searchAssigned, userId, role);
            }
            else
            {
                leads = await _leadService.GetAllAsync(userId, role);
            }

            ViewBag.SearchName = searchName;
            ViewBag.SearchCompany = searchCompany;
            ViewBag.SearchStatus = searchStatus;
            ViewBag.SearchAssigned = searchAssigned;
            await PopulateDropdowns();
            return View(leads);
        }

        public async Task<IActionResult> Details(int id)
        {
            var lead = await _leadService.GetByIdAsync(id);
            if (lead == null) return NotFound();
            return View(lead);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new LeadViewModel { Status = "New", Source = "Website", Priority = "Medium" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            var validStatuses = new[] { "New", "Contacted", "Qualified", "Unqualified", "Converted", "Lost" };
            if (!validStatuses.Contains(model.Status))
            {
                ModelState.AddModelError("Status", "Invalid lead status selected.");
                await PopulateDropdowns();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var lead = new Lead
            {
                LeadName = model.LeadName,
                Email = model.Email,
                Phone = model.Phone,
                CompanyName = model.CompanyName,
                Source = model.Source,
                Status = model.Status,
                Priority = model.Priority,
                ExpectedValue = model.ExpectedValue,
                AssignedTo = !string.IsNullOrEmpty(model.AssignedTo) ? model.AssignedTo : user!.Id,
                Notes = model.Notes
            };

            var created = await _leadService.CreateAsync(lead);
            await _auditService.LogAsync(user!.Id, "Create", "Lead", created.LeadId.ToString(), null, lead.LeadName, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = $"Lead '{lead.LeadName}' created successfully with code {created.LeadCode}.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var lead = await _leadService.GetByIdAsync(id);
            if (lead == null) return NotFound();
            await PopulateDropdowns();
            var model = new LeadViewModel
            {
                LeadId = lead.LeadId,
                LeadName = lead.LeadName,
                Email = lead.Email,
                Phone = lead.Phone,
                CompanyName = lead.CompanyName,
                Source = lead.Source,
                Status = lead.Status,
                Priority = lead.Priority,
                ExpectedValue = lead.ExpectedValue,
                AssignedTo = lead.AssignedTo,
                Notes = lead.Notes
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LeadViewModel model)
        {
            if (id != model.LeadId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            var lead = await _leadService.GetByIdAsync(id);
            if (lead == null) return NotFound();

            if (lead.Status != model.Status && !_leadService.IsValidStatusTransition(lead.Status, model.Status))
            {
                ModelState.AddModelError("Status", $"Invalid workflow transition from '{lead.Status}' to '{model.Status}'.");
                await PopulateDropdowns();
                return View(model);
            }

            var oldValue = $"{lead.LeadName} (Status: {lead.Status})";
            lead.LeadName = model.LeadName;
            lead.Email = model.Email;
            lead.Phone = model.Phone;
            lead.CompanyName = model.CompanyName;
            lead.Source = model.Source;
            lead.Status = model.Status;
            lead.Priority = model.Priority;
            lead.ExpectedValue = model.ExpectedValue;
            lead.AssignedTo = model.AssignedTo;
            lead.Notes = model.Notes;

            await _leadService.UpdateAsync(lead);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Update", "Lead", id.ToString(), oldValue, $"{lead.LeadName} (Status: {lead.Status})", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Lead updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lead = await _leadService.GetByIdAsync(id);
            if (lead == null) return NotFound();
            await _leadService.DeleteAsync(id);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Delete", "Lead", id.ToString(), lead.LeadName, "Status changed to Lost", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = $"Lead '{lead.LeadName}' marked as Lost.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,SalesExecutive")]
        public async Task<IActionResult> Convert(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _leadService.ConvertToCustomerAsync(id, userId!);
            if (result == null)
            {
                TempData["Error"] = "Lead cannot be converted. It must be in 'Qualified' status first.";
                return RedirectToAction(nameof(Index));
            }
            await _auditService.LogAsync(userId, "Convert", "Lead", id.ToString(), "Qualified", "Converted to Customer ID: " + result.CustomerId, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = $"Lead '{result.LeadName}' converted to customer successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

