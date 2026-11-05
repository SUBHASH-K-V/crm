using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class FollowUpService : IFollowUpService
    {
        private readonly ApplicationDbContext _context;

        public FollowUpService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FollowUp>> GetAllAsync(string? userId = null, string? role = null)
        {
            var query = _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead).Include(f => f.Opportunity).Include(f => f.AssignedUser).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(f => f.AssignedTo == userId);
            return await query.OrderByDescending(f => f.FollowUpDate).ToListAsync();
        }

        public async Task<FollowUp?> GetByIdAsync(int id)
        {
            return await _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead).Include(f => f.Opportunity).Include(f => f.AssignedUser).FirstOrDefaultAsync(f => f.FollowUpId == id);
        }

        public async Task<FollowUp> CreateAsync(FollowUp followUp)
        {
            followUp.CreatedDate = DateTime.UtcNow;
            _context.FollowUps.Add(followUp);
            await _context.SaveChangesAsync();
            return followUp;
        }

        public async Task UpdateAsync(FollowUp followUp)
        {
            _context.FollowUps.Update(followUp);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<FollowUp>> GetUpcomingAsync(string? userId = null, string? role = null)
        {
            var query = _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead).Include(f => f.AssignedUser)
                .Where(f => f.Status == "Planned" && f.FollowUpDate >= DateTime.Today).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(f => f.AssignedTo == userId);
            return await query.OrderBy(f => f.FollowUpDate).ToListAsync();
        }

        public async Task<IEnumerable<FollowUp>> GetOverdueAsync(string? userId = null, string? role = null)
        {
            var query = _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead).Include(f => f.AssignedUser)
                .Where(f => f.Status == "Planned" && f.FollowUpDate < DateTime.Today).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(f => f.AssignedTo == userId);
            return await query.OrderBy(f => f.FollowUpDate).ToListAsync();
        }

        public async Task<IEnumerable<FollowUp>> SearchAsync(DateTime? date, string? status, string? assignedTo, string? userId = null, string? role = null)
        {
            var query = _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead).Include(f => f.AssignedUser).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(f => f.AssignedTo == userId);
            if (date.HasValue)
                query = query.Where(f => f.FollowUpDate.Date == date.Value.Date);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(f => f.Status == status);
            if (!string.IsNullOrEmpty(assignedTo))
                query = query.Where(f => f.AssignedTo == assignedTo);
            return await query.OrderByDescending(f => f.FollowUpDate).ToListAsync();
        }
    }
}

