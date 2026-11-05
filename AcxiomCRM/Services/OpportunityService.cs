using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class OpportunityService : IOpportunityService
    {
        private readonly ApplicationDbContext _context;

        public OpportunityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Opportunity>> GetAllAsync(string? userId = null, string? role = null)
        {
            var query = _context.Opportunities.Include(o => o.Customer).Include(o => o.AssignedUser).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(o => o.AssignedTo == userId);
            return await query.OrderByDescending(o => o.CreatedDate).ToListAsync();
        }

        public async Task<Opportunity?> GetByIdAsync(int id)
        {
            return await _context.Opportunities.Include(o => o.Customer).Include(o => o.AssignedUser).Include(o => o.Lead).FirstOrDefaultAsync(o => o.OpportunityId == id);
        }

        public async Task<Opportunity> CreateAsync(Opportunity opportunity)
        {
            opportunity.CreatedDate = DateTime.UtcNow;
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            return opportunity;
        }

        public async Task UpdateAsync(Opportunity opportunity)
        {
            opportunity.ModifiedDate = DateTime.UtcNow;
            _context.Opportunities.Update(opportunity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var opp = await _context.Opportunities.FindAsync(id);
            if (opp != null)
            {
                opp.Status = "Closed";
                opp.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Opportunity>> SearchAsync(string? name, int? customerId, string? stage, string? status, string? userId = null, string? role = null)
        {
            var query = _context.Opportunities.Include(o => o.Customer).Include(o => o.AssignedUser).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(o => o.AssignedTo == userId);
            if (!string.IsNullOrEmpty(name))
                query = query.Where(o => o.OpportunityName.Contains(name));
            if (customerId.HasValue)
                query = query.Where(o => o.CustomerId == customerId);
            if (!string.IsNullOrEmpty(stage))
                query = query.Where(o => o.Stage == stage);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);
            return await query.OrderByDescending(o => o.CreatedDate).ToListAsync();
        }
    }
}

