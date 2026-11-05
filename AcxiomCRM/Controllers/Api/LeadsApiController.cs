using AcxiomCRM.DTOs;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers.Api
{
    [Route("api/leads")]
    [ApiController]
    [Authorize]
    public class LeadsApiController : ControllerBase
    {
        private readonly ILeadService _leadService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeadsApiController(
            ILeadService leadService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _leadService = leadService;
            _auditService = auditService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            var role = roles.FirstOrDefault() ?? "SalesExecutive";
            var leads = await _leadService.GetAllAsync(role == "Admin" ? null : user!.Id, role);

            var dtos = leads.Select(l => new LeadDto
            {
                LeadId = l.LeadId,
                LeadCode = l.LeadCode,
                LeadName = l.LeadName,
                Email = l.Email,
                Phone = l.Phone,
                CompanyName = l.CompanyName,
                Source = l.Source,
                Status = l.Status,
                Priority = l.Priority,
                ExpectedValue = l.ExpectedValue,
                AssignedToName = l.AssignedUser?.FullName,
                CreatedDate = l.CreatedDate
            });

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeadDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body cannot be empty." });

            if (string.IsNullOrWhiteSpace(dto.LeadName))
                return BadRequest(new { message = "Lead Name is required." });

            var validStatuses = new[] { "New", "Contacted", "Qualified", "Unqualified", "Converted", "Lost" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Invalid lead status. Allowed: New, Contacted, Qualified, Unqualified, Converted, Lost" });

            if (dto.ExpectedValue < 0 || dto.ExpectedValue > 100000000)
                return BadRequest(new { message = "Expected value must be between 0 and 100,000,000." });

            var user = await _userManager.GetUserAsync(User);
            var lead = new Lead
            {
                LeadName = dto.LeadName,
                Email = dto.Email,
                Phone = dto.Phone,
                CompanyName = dto.CompanyName,
                Source = string.IsNullOrEmpty(dto.Source) ? "Website" : dto.Source,
                Status = string.IsNullOrEmpty(dto.Status) ? "New" : dto.Status,
                Priority = string.IsNullOrEmpty(dto.Priority) ? "Medium" : dto.Priority,
                ExpectedValue = dto.ExpectedValue,
                AssignedTo = user!.Id,
                Notes = dto.Notes
            };

            var created = await _leadService.CreateAsync(lead);
            await _auditService.LogAsync(user.Id, "Create", "Lead", created.LeadId.ToString(), null, lead.LeadName, HttpContext.Connection.RemoteIpAddress?.ToString());

            return CreatedAtAction(nameof(GetAll), new { id = created.LeadId }, new LeadDto
            {
                LeadId = created.LeadId,
                LeadCode = created.LeadCode,
                LeadName = created.LeadName,
                Email = created.Email,
                Phone = created.Phone,
                CompanyName = created.CompanyName,
                Source = created.Source,
                Status = created.Status,
                Priority = created.Priority,
                ExpectedValue = created.ExpectedValue,
                AssignedToName = user.FullName,
                CreatedDate = created.CreatedDate
            });
        }
    }
}

