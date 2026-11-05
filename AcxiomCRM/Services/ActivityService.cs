using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _context;

        public ActivityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Activity>> GetAllAsync(string? userId = null, string? role = null)
        {
            var query = _context.Activities.Include(a => a.Customer).Include(a => a.Lead).Include(a => a.AssignedUser).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(a => a.AssignedTo == userId);
            return await query.OrderByDescending(a => a.ActivityDate).ToListAsync();
        }

        public async Task<Activity?> GetByIdAsync(int id)
        {
            return await _context.Activities.Include(a => a.Customer).Include(a => a.Lead).Include(a => a.AssignedUser).FirstOrDefaultAsync(a => a.ActivityId == id);
        }

        public async Task<Activity> CreateAsync(Activity activity)
        {
            activity.CreatedDate = DateTime.UtcNow;
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            return activity;
        }

        public async Task UpdateAsync(Activity activity)
        {
            _context.Activities.Update(activity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity != null)
            {
                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Activity>> SearchAsync(string? type, DateTime? date, string? status, string? assignedTo, string? userId = null, string? role = null)
        {
            var query = _context.Activities.Include(a => a.Customer).Include(a => a.Lead).Include(a => a.AssignedUser).AsQueryable();
            if (role == "SalesExecutive" && userId != null)
                query = query.Where(a => a.AssignedTo == userId);
            if (!string.IsNullOrEmpty(type))
                query = query.Where(a => a.ActivityType == type);
            if (date.HasValue)
                query = query.Where(a => a.ActivityDate.Date == date.Value.Date);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);
            if (!string.IsNullOrEmpty(assignedTo))
                query = query.Where(a => a.AssignedTo == assignedTo);
            return await query.OrderByDescending(a => a.ActivityDate).ToListAsync();
        }
    }
}

