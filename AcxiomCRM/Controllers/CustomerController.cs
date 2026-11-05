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
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ICustomerService customerService, IAuditService auditService, UserManager<ApplicationUser> userManager)
        {
            _customerService = customerService;
            _auditService = auditService;
            _userManager = userManager;
        }

        private async Task<(string userId, string role)> GetUserContextAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            return (user!.Id, roles.FirstOrDefault() ?? "SalesExecutive");
        }

        private async Task PopulateSalesExecutives()
        {
            var users = _userManager.Users.Where(u => u.IsActive).ToList();
            ViewBag.SalesExecutives = users.Select(u => new SelectListItem { Value = u.Id, Text = u.FullName + " (" + u.Email + ")" }).ToList();
        }

        public async Task<IActionResult> Index(string? searchName, string? searchEmail, string? searchPhone, string? searchCompany)
        {
            var (userId, role) = await GetUserContextAsync();
            IEnumerable<Customer> customers;
            if (!string.IsNullOrWhiteSpace(searchName) || !string.IsNullOrWhiteSpace(searchEmail) || 
                !string.IsNullOrWhiteSpace(searchPhone) || !string.IsNullOrWhiteSpace(searchCompany))
            {
                customers = await _customerService.SearchAsync(searchName, searchEmail, searchPhone, searchCompany, userId, role);
            }
            else
            {
                customers = await _customerService.GetAllAsync(userId, role);
            }

            ViewBag.SearchName = searchName;
            ViewBag.SearchEmail = searchEmail;
            ViewBag.SearchPhone = searchPhone;
            ViewBag.SearchCompany = searchCompany;
            return View(customers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateSalesExecutives();
            return View(new CustomerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSalesExecutives();
                return View(model);
            }

            if (await _customerService.IsDuplicateEmailAsync(model.Email))
            {
                ModelState.AddModelError("Email", "A customer with this email address already exists.");
                await PopulateSalesExecutives();
                return View(model);
            }

            if (await _customerService.IsDuplicatePhoneAsync(model.Phone))
            {
                ModelState.AddModelError("Phone", "A customer with this phone number already exists.");
                await PopulateSalesExecutives();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var customer = new Customer
            {
                CustomerName = model.CustomerName,
                Email = model.Email,
                Phone = model.Phone,
                CompanyName = model.CompanyName,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Status = model.Status,
                OwnerId = !string.IsNullOrEmpty(model.OwnerId) ? model.OwnerId : user!.Id,
                CreatedBy = user!.Id,
                Notes = model.Notes
            };

            var created = await _customerService.CreateAsync(customer);
            await _auditService.LogAsync(user.Id, "Create", "Customer", created.CustomerId.ToString(), null, customer.CustomerName, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = $"Customer '{customer.CustomerName}' created successfully with code {created.CustomerCode}.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            await PopulateSalesExecutives();
            var model = new CustomerViewModel
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,
                Email = customer.Email,
                Phone = customer.Phone,
                CompanyName = customer.CompanyName,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                Status = customer.Status,
                OwnerId = customer.OwnerId,
                Notes = customer.Notes
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerViewModel model)
        {
            if (id != model.CustomerId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateSalesExecutives();
                return View(model);
            }

            if (await _customerService.IsDuplicateEmailAsync(model.Email, id))
            {
                ModelState.AddModelError("Email", "A customer with this email address already exists.");
                await PopulateSalesExecutives();
                return View(model);
            }

            if (await _customerService.IsDuplicatePhoneAsync(model.Phone, id))
            {
                ModelState.AddModelError("Phone", "A customer with this phone number already exists.");
                await PopulateSalesExecutives();
                return View(model);
            }

            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            var oldValue = $"{customer.CustomerName} ({customer.Email})";

            customer.CustomerName = model.CustomerName;
            customer.Email = model.Email;
            customer.Phone = model.Phone;
            customer.CompanyName = model.CompanyName;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.State = model.State;
            customer.Status = model.Status;
            customer.OwnerId = model.OwnerId;
            customer.Notes = model.Notes;

            await _customerService.UpdateAsync(customer);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Update", "Customer", id.ToString(), oldValue, $"{customer.CustomerName} ({customer.Email})", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            await _customerService.DeleteAsync(id);
            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(userId, "Delete", "Customer", id.ToString(), customer.CustomerName, "Deactivated/Inactive", HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Success"] = $"Customer '{customer.CustomerName}' has been deactivated.";
            return RedirectToAction(nameof(Index));
        }
    }
}

