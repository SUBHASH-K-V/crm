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
    public class OpportunityController : Controller
    {
        private readonly IOpportunityService _opportunityService;
        private readonly ICustomerService _customerService;
        private readonly ILeadService _leadService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OpportunityController(
            IOpportunityService opportunityService,
            ICustomerService customerService,
            ILeadService leadService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _opportunityService = opportunityService;
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
            ViewBag.Customers = customers.Select(c => new SelectListItem { Value = c.CustomerId.ToString(), Text = c.CustomerName + " (" + c.CustomerCode + ")" }).ToList();
            
            var leads = await _leadService.GetAllAsync();
            ViewBag.Leads = leads.Select(l => new SelectListItem { Value = l.LeadId.ToString(), Text = l.LeadName + " (" + l.LeadCode + ")" }).ToList();

            var users = _userManager.Users.Where(u => u.IsActive).ToList();
            ViewBag.SalesExecutives = users.Select(u => new SelectListItem { Value = u.Id, Text = u.FullName + " (" + u.Email + ")" }).ToList();

            ViewBag.Stages = new List<SelectListItem>
            {
                new("Qualification", "Qualification"),
                new("Proposal", "Proposal"),
                new("Negotiation", "Negotiation"),
                new("Won", "Won"),
                new("Lost", "Lost")
            };

            ViewBag.Statuses = new List<SelectListItem>
            {
                new("Open", "Open"),
                new("Closed", "Closed")
            };
        }

        public async Task<IActionResult> Index(string? searchName, int? searchCustomer, string? searchStage, string? searchStatus)
        {
            var (userId, role) = await GetUserContextAsync();
            IEnumerable<Opportunity> opps;
            if (!string.IsNullOrWhiteSpace(searchName) || searchCustomer.HasValue || 
                !string.IsNullOrWhiteSpace(searchStage) || !string.IsNullOrWhiteSpace(searchStatus))
            {
                opps = await _opportunityService.SearchAsync(searchName, searchCustomer, searchStage, searchStatus, userId, role);
            }
            else
            {
                opps = await _opportunityService.GetAllAsync(userId, role);
            }

            ViewBag.SearchName = searchName;
            ViewBag.SearchCustomer = searchCustomer;
            ViewBag.SearchStage = searchStage;
            ViewBag.SearchStatus = searchStatus;
            await PopulateDropdowns();
            return View(opps);
        }

        public async Task<IActionResult> Details(int id)
        {
            var opp = await _opportunityService.GetByIdAsync(id);
            if (opp == null) return NotFound();
            return View(opp);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new OpportunityViewModel 
            { 
                ExpectedCloseDate = DateTime.Today.AddDays(30), 
                Probability = 20, 
                Stage = "Qualification", 
                Status = "Open" 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OpportunityViewModel model)
        {
            // Business Validation
            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Opportunity Amount must be greater than 0.");
            }
            if (model.Probability < 0 || model.Probability > 100)
            {
                ModelState.AddModelError("Probability", "Probability must be between 0 and 100.");
            }
            if (model.ExpectedCloseDate.Date < DateTime.Today && model.Status != "Closed")
            {
                ModelState.AddModelError("ExpectedCloseDate", "Expected Close Date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var opportunity = new Opportunity
            {
                OpportunityName = model.OpportunityName,
                CustomerId = model.CustomerId,
                LeadId = model.LeadId,
                Amount = model.Amount,
                Stage = model.Stage,
                Probability = model.Probability,
                ExpectedCloseDate = model.ExpectedCloseDate,
                Status = model.Status,
                AssignedTo = !string.IsNullOrEmpty(model.AssignedTo) ? model.AssignedTo : user!.Id,
                Notes = model.Notes
            };

            var created = await _opportunityService.CreateAsync(opportunity);
            await _auditService.LogAsync(user!.Id, "Create", "Opportunity", created.OpportunityId.ToString(), null, opportunity.OpportunityName, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = $"Opportunity '{opportunity.OpportunityName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var opp = await _opportunityService.GetByIdAsync(id);
            if (opp == null) return NotFound();
            await PopulateDropdowns();
            var model = new OpportunityViewModel
            {
                OpportunityId = opp.OpportunityId,
                OpportunityName = opp.OpportunityName,
                CustomerId = opp.CustomerId,
                LeadId = opp.LeadId,
                Amount = opp.Amount,
                Stage = opp.Stage,
                Probability = opp.Probability,
                ExpectedCloseDate = opp.ExpectedCloseDate,
                Status = opp.Status,
                AssignedTo = opp.AssignedTo,
                Notes = opp.Notes
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OpportunityViewModel model)
        {
            if (id != model.OpportunityId) return BadRequest();

            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Opportunity Amount must be greater than 0.");
            }
            if (model.Probability < 0 || model.Probability > 100)
            {
                ModelState.AddModelError("Probability", "Probability must be between 0 and 100.");
            }
            if (model.ExpectedCloseDate.Date < DateTime.Today && model.Status == "Open")
            {
                ModelState.AddModelError("ExpectedCloseDate", "Expected Close Date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            var opp = await _opportunityService.GetByIdAsync(id);
            if (opp == null) return NotFound();
            var oldValue = $"{opp.OpportunityName} (Stage: {opp.Stage}, Amount: {opp.Amount})";

            opp.OpportunityName = model.OpportunityName;
            opp.CustomerId = model.CustomerId;
            opp.LeadId = model.LeadId;
            opp.Amount = model.Amount;
            opp.Stage = model.Stage;
            opp.Probability = model.Probability;
            opp.ExpectedCloseDate = model.ExpectedCloseDate;
            opp.Status = model.Status;
            opp.AssignedTo = model.AssignedTo;
            opp.Notes = model.Notes;

            await _opportunityService.UpdateAsync(opp);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Update", "Opportunity", id.ToString(), oldValue, $"{model.OpportunityName} (Stage: {model.Stage}, Amount: {model.Amount})", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Opportunity updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var opp = await _opportunityService.GetByIdAsync(id);
            if (opp == null) return NotFound();
            await _opportunityService.DeleteAsync(id);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Delete", "Opportunity", id.ToString(), opp.OpportunityName, "Marked as Closed", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = $"Opportunity '{opp.OpportunityName}' has been closed.";
            return RedirectToAction(nameof(Index));
        }
    }
}

