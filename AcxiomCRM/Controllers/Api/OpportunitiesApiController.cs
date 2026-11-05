using AcxiomCRM.DTOs;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers.Api
{
    [Route("api/opportunities")]
    [ApiController]
    [Authorize]
    public class OpportunitiesApiController : ControllerBase
    {
        private readonly IOpportunityService _opportunityService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OpportunitiesApiController(
            IOpportunityService opportunityService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _opportunityService = opportunityService;
            _auditService = auditService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            var role = roles.FirstOrDefault() ?? "SalesExecutive";
            var opps = await _opportunityService.GetAllAsync(role == "Admin" ? null : user!.Id, role);

            var dtos = opps.Select(o => new OpportunityDto
            {
                OpportunityId = o.OpportunityId,
                OpportunityName = o.OpportunityName,
                CustomerName = o.Customer?.CustomerName,
                Amount = o.Amount,
                Stage = o.Stage,
                Probability = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate,
                Status = o.Status,
                WeightedValue = o.WeightedValue,
                AssignedToName = o.AssignedUser?.FullName,
                CreatedDate = o.CreatedDate
            });

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body cannot be empty." });

            if (string.IsNullOrWhiteSpace(dto.OpportunityName))
                return BadRequest(new { message = "Opportunity Name is required." });

            if (dto.Amount <= 0)
                return BadRequest(new { message = "Opportunity Amount must be greater than 0." });

            if (dto.Probability < 0 || dto.Probability > 100)
                return BadRequest(new { message = "Probability must be between 0 and 100." });

            if (dto.ExpectedCloseDate.Date < DateTime.Today)
                return BadRequest(new { message = "Expected Close Date cannot be in the past." });

            var user = await _userManager.GetUserAsync(User);
            var opp = new Opportunity
            {
                OpportunityName = dto.OpportunityName,
                CustomerId = dto.CustomerId,
                LeadId = dto.LeadId,
                Amount = dto.Amount,
                Stage = string.IsNullOrEmpty(dto.Stage) ? "Qualification" : dto.Stage,
                Probability = dto.Probability,
                ExpectedCloseDate = dto.ExpectedCloseDate,
                Status = "Open",
                AssignedTo = user!.Id,
                Notes = dto.Notes
            };

            var created = await _opportunityService.CreateAsync(opp);
            await _auditService.LogAsync(user.Id, "Create", "Opportunity", created.OpportunityId.ToString(), null, opp.OpportunityName, HttpContext.Connection.RemoteIpAddress?.ToString());

            return CreatedAtAction(nameof(GetAll), new { id = created.OpportunityId }, new OpportunityDto
            {
                OpportunityId = created.OpportunityId,
                OpportunityName = created.OpportunityName,
                Amount = created.Amount,
                Stage = created.Stage,
                Probability = created.Probability,
                ExpectedCloseDate = created.ExpectedCloseDate,
                Status = created.Status,
                WeightedValue = created.WeightedValue,
                CreatedDate = created.CreatedDate
            });
        }
    }
}

