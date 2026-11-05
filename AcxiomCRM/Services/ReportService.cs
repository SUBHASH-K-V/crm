using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetCustomerReportAsync(string? status = null, string? ownerId = null)
        {
            var query = _context.Customers.Include(c => c.Owner).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(c => c.Status == status);
            if (!string.IsNullOrEmpty(ownerId)) query = query.Where(c => c.OwnerId == ownerId);
            return await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
        }

        public async Task<IEnumerable<Lead>> GetLeadReportAsync(string? source = null, string? status = null, string? ownerId = null)
        {
            var query = _context.Leads.Include(l => l.AssignedUser).AsQueryable();
            if (!string.IsNullOrEmpty(source)) query = query.Where(l => l.Source == source);
            if (!string.IsNullOrEmpty(status)) query = query.Where(l => l.Status == status);
            if (!string.IsNullOrEmpty(ownerId)) query = query.Where(l => l.AssignedTo == ownerId);
            return await query.OrderByDescending(l => l.CreatedDate).ToListAsync();
        }

        public async Task<IEnumerable<FollowUp>> GetFollowUpReportAsync(string? status = null, string? assignedTo = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.FollowUps.Include(f => f.Customer).Include(f => f.Lead).Include(f => f.AssignedUser).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(f => f.Status == status);
            if (!string.IsNullOrEmpty(assignedTo)) query = query.Where(f => f.AssignedTo == assignedTo);
            if (fromDate.HasValue) query = query.Where(f => f.FollowUpDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(f => f.FollowUpDate <= toDate.Value);
            return await query.OrderByDescending(f => f.FollowUpDate).ToListAsync();
        }

        public async Task<IEnumerable<Opportunity>> GetOpportunityReportAsync(string? stage = null, string? status = null, string? ownerId = null)
        {
            var query = _context.Opportunities.Include(o => o.Customer).Include(o => o.AssignedUser).AsQueryable();
            if (!string.IsNullOrEmpty(stage)) query = query.Where(o => o.Stage == stage);
            if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);
            if (!string.IsNullOrEmpty(ownerId)) query = query.Where(o => o.AssignedTo == ownerId);
            return await query.OrderByDescending(o => o.CreatedDate).ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetPipelineReportAsync()
        {
            var openOpps = await _context.Opportunities
                .Where(o => o.Status == "Open")
                .ToListAsync();

            return openOpps
                .GroupBy(o => o.Stage)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.Amount));
        }

        public async Task<IEnumerable<AuditLog>> GetAuditReportAsync(string? userId = null, string? action = null, string? module = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs.Include(a => a.User).AsQueryable();
            if (!string.IsNullOrEmpty(userId)) query = query.Where(a => a.UserId == userId);
            if (!string.IsNullOrEmpty(action)) query = query.Where(a => a.Action == action);
            if (!string.IsNullOrEmpty(module)) query = query.Where(a => a.EntityName == module);
            if (fromDate.HasValue) query = query.Where(a => a.CreatedDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(a => a.CreatedDate <= toDate.Value);
            return await query.OrderByDescending(a => a.CreatedDate).ToListAsync();
        }
    }
}
