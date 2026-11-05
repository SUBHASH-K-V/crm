using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(string? userId = null, string? role = null)
        {
            var query = _context.Customers.Include(c => c.Owner).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(c => c.OwnerId == userId);
            return await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.Include(c => c.Owner).FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            customer.CustomerCode = "CUST-" + (await _context.Customers.CountAsync() + 1).ToString("D5");
            customer.CreatedDate = DateTime.UtcNow;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            customer.ModifiedDate = DateTime.UtcNow;
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                customer.Status = "Inactive";
                customer.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsDuplicateEmailAsync(string email, int? excludeId = null)
        {
            return await _context.Customers.AnyAsync(c => c.Email == email && (excludeId == null || c.CustomerId != excludeId));
        }

        public async Task<bool> IsDuplicatePhoneAsync(string phone, int? excludeId = null)
        {
            return await _context.Customers.AnyAsync(c => c.Phone == phone && (excludeId == null || c.CustomerId != excludeId));
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string? name, string? email, string? phone, string? company, string? userId = null, string? role = null)
        {
            var query = _context.Customers.Include(c => c.Owner).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(c => c.OwnerId == userId);
            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.CustomerName.Contains(name));
            if (!string.IsNullOrEmpty(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!string.IsNullOrEmpty(phone))
                query = query.Where(c => c.Phone.Contains(phone));
            if (!string.IsNullOrEmpty(company))
                query = query.Where(c => c.CompanyName != null && c.CompanyName.Contains(company));
            return await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
        }
    }
}

