using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class LeadService : ILeadService
    {
        private readonly ApplicationDbContext _context;
        private static readonly Dictionary<string, List<string>> ValidTransitions = new()
        {
            { "New", new List<string> { "Contacted", "Unqualified", "Lost" } },
            { "Contacted", new List<string> { "Qualified", "Unqualified", "Lost" } },
            { "Qualified", new List<string> { "Converted", "Lost" } },
            { "Unqualified", new List<string> { "New", "Lost" } },
            { "Converted", new List<string>() },
            { "Lost", new List<string> { "New" } }
        };

        public LeadService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lead>> GetAllAsync(string? userId = null, string? role = null)
        {
            var query = _context.Leads.Include(l => l.AssignedUser).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(l => l.AssignedTo == userId);
            return await query.OrderByDescending(l => l.CreatedDate).ToListAsync();
        }

        public async Task<Lead?> GetByIdAsync(int id)
        {
            return await _context.Leads.Include(l => l.AssignedUser).Include(l => l.Customer).FirstOrDefaultAsync(l => l.LeadId == id);
        }

        public async Task<Lead> CreateAsync(Lead lead)
        {
            lead.LeadCode = "LEAD-" + (await _context.Leads.CountAsync() + 1).ToString("D5");
            lead.CreatedDate = DateTime.UtcNow;
            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();
            return lead;
        }

        public async Task UpdateAsync(Lead lead)
        {
            _context.Leads.Update(lead);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var lead = await _context.Leads.FindAsync(id);
            if (lead != null)
            {
                lead.Status = "Lost";
                await _context.SaveChangesAsync();
            }
        }

        public bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            if (ValidTransitions.TryGetValue(currentStatus, out var allowed))
                return allowed.Contains(newStatus);
            return false;
        }

        public async Task<Lead?> ConvertToCustomerAsync(int leadId, string userId)
        {
            var lead = await _context.Leads.FindAsync(leadId);
            if (lead == null || lead.Status != "Qualified") return null;

            var customer = new Customer
            {
                CustomerCode = "CUST-" + (await _context.Customers.CountAsync() + 1).ToString("D5"),
                CustomerName = lead.LeadName,
                Email = lead.Email,
                Phone = lead.Phone,
                CompanyName = lead.CompanyName,
                Status = "Active",
                CreatedBy = userId,
                OwnerId = lead.AssignedTo,
                CreatedDate = DateTime.UtcNow
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            lead.Status = "Converted";
            lead.CustomerId = customer.CustomerId;
            await _context.SaveChangesAsync();
            return lead;
        }

        public async Task<IEnumerable<Lead>> SearchAsync(string? name, string? company, string? status, string? assignedTo, string? userId = null, string? role = null)
        {
            var query = _context.Leads.Include(l => l.AssignedUser).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(l => l.AssignedTo == userId);
            if (!string.IsNullOrEmpty(name))
                query = query.Where(l => l.LeadName.Contains(name));
            if (!string.IsNullOrEmpty(company))
                query = query.Where(l => l.CompanyName != null && l.CompanyName.Contains(company));
            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.Status == status);
            if (!string.IsNullOrEmpty(assignedTo))
                query = query.Where(l => l.AssignedTo == assignedTo);
            return await query.OrderByDescending(l => l.CreatedDate).ToListAsync();
        }
    }
}

