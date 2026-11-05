using AcxiomCRM.DTOs;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AcxiomCRM.Controllers.Api
{
    [Route("api/customers")]
    [ApiController]
    [Authorize]
    public class CustomersApiController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersApiController(
            ICustomerService customerService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _customerService = customerService;
            _auditService = auditService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            var role = roles.FirstOrDefault() ?? "SalesExecutive";
            var customers = await _customerService.GetAllAsync(role == "Admin" ? null : user!.Id, role);

            var dtos = customers.Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                CompanyName = c.CompanyName,
                Address = c.Address,
                City = c.City,
                State = c.State,
                Status = c.Status,
                OwnerName = c.Owner?.FullName,
                CreatedDate = c.CreatedDate
            });

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound(new { message = $"Customer with ID {id} not found." });

            var dto = new CustomerDto
            {
                CustomerId = customer.CustomerId,
                CustomerCode = customer.CustomerCode,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                Phone = customer.Phone,
                CompanyName = customer.CompanyName,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                Status = customer.Status,
                OwnerName = customer.Owner?.FullName,
                CreatedDate = customer.CreatedDate
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body cannot be empty." });

            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest(new { message = "Customer Name is required." });

            if (string.IsNullOrWhiteSpace(dto.Email) || !new EmailAddressAttribute().IsValid(dto.Email))
                return BadRequest(new { message = "Enter a valid email address." });

            if (string.IsNullOrWhiteSpace(dto.Phone) || !Regex.IsMatch(dto.Phone, @"^[6-9]\d{9}$"))
                return BadRequest(new { message = "Enter a valid 10-digit Indian mobile number." });

            if (await _customerService.IsDuplicateEmailAsync(dto.Email))
                return Conflict(new { message = "A customer with this email already exists." });

            if (await _customerService.IsDuplicatePhoneAsync(dto.Phone))
                return Conflict(new { message = "A customer with this phone number already exists." });

            var user = await _userManager.GetUserAsync(User);
            var customer = new Customer
            {
                CustomerName = dto.CustomerName,
                Email = dto.Email,
                Phone = dto.Phone,
                CompanyName = dto.CompanyName,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                CreatedBy = user!.Id,
                OwnerId = user.Id,
                Status = "Active",
                Notes = dto.Notes
            };

            var created = await _customerService.CreateAsync(customer);
            await _auditService.LogAsync(user.Id, "Create", "Customer", created.CustomerId.ToString(), null, customer.CustomerName, HttpContext.Connection.RemoteIpAddress?.ToString());

            return CreatedAtAction(nameof(GetById), new { id = created.CustomerId }, new CustomerDto
            {
                CustomerId = created.CustomerId,
                CustomerCode = created.CustomerCode,
                CustomerName = created.CustomerName,
                Email = created.Email,
                Phone = created.Phone,
                CompanyName = created.CompanyName,
                Address = created.Address,
                City = created.City,
                State = created.State,
                Status = created.Status,
                OwnerName = user.FullName,
                CreatedDate = created.CreatedDate
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body cannot be empty." });

            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound(new { message = $"Customer with ID {id} not found." });

            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest(new { message = "Customer Name is required." });

            if (string.IsNullOrWhiteSpace(dto.Email) || !new EmailAddressAttribute().IsValid(dto.Email))
                return BadRequest(new { message = "Enter a valid email address." });

            if (string.IsNullOrWhiteSpace(dto.Phone) || !Regex.IsMatch(dto.Phone, @"^[6-9]\d{9}$"))
                return BadRequest(new { message = "Enter a valid 10-digit Indian mobile number." });

            if (await _customerService.IsDuplicateEmailAsync(dto.Email, id))
                return Conflict(new { message = "A customer with this email already exists." });

            if (await _customerService.IsDuplicatePhoneAsync(dto.Phone, id))
                return Conflict(new { message = "A customer with this phone number already exists." });

            customer.CustomerName = dto.CustomerName;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
            customer.CompanyName = dto.CompanyName;
            customer.Address = dto.Address;
            customer.City = dto.City;
            customer.State = dto.State;
            customer.Status = dto.Status;
            customer.Notes = dto.Notes;

            await _customerService.UpdateAsync(customer);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Update", "Customer", id.ToString(), null, dto.CustomerName, HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Customer updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound(new { message = $"Customer with ID {id} not found." });

            await _customerService.DeleteAsync(id);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Delete", "Customer", id.ToString(), customer.CustomerName, "Deactivated", HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Customer deactivated successfully." });
        }
    }
}

