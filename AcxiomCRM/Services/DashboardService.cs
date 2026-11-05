using AcxiomCRM.Data;
using AcxiomCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string? userId = null, string? role = null)
        {
            var vm = new DashboardViewModel();

            var customerQuery = _context.Customers.AsQueryable();
            var leadQuery = _context.Leads.AsQueryable();
            var oppQuery = _context.Opportunities.AsQueryable();
            var followUpQuery = _context.FollowUps.AsQueryable();

            if (role == "SalesExecutive" && userId != null)
            {
                customerQuery = customerQuery.Where(c => c.OwnerId == userId);
                leadQuery = leadQuery.Where(l => l.AssignedTo == userId);
                oppQuery = oppQuery.Where(o => o.AssignedTo == userId);
                followUpQuery = followUpQuery.Where(f => f.AssignedTo == userId);
            }

            vm.TotalCustomers = await customerQuery.CountAsync();
            vm.TotalLeads = await leadQuery.CountAsync();
            vm.OpenLeads = await leadQuery.Where(l => l.Status != "Converted" && l.Status != "Lost").CountAsync();
            vm.TotalOpportunities = await oppQuery.CountAsync();
            vm.OpenOpportunities = await oppQuery.Where(o => o.Status == "Open").CountAsync();
            vm.WonOpportunities = await oppQuery.Where(o => o.Stage == "Won").CountAsync();
            vm.LostOpportunities = await oppQuery.Where(o => o.Stage == "Lost").CountAsync();
            vm.TotalPipelineValue = await oppQuery.Where(o => o.Status == "Open").SumAsync(o => o.Amount);
            vm.PendingFollowUps = await followUpQuery.Where(f => f.Status == "Planned" && f.FollowUpDate >= DateTime.Today).CountAsync();
            vm.OverdueFollowUps = await followUpQuery.Where(f => f.Status == "Planned" && f.FollowUpDate < DateTime.Today).CountAsync();

            // Lead Status Chart Data
            vm.LeadStatusData = await leadQuery.GroupBy(l => l.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            // Opportunity Pipeline Chart Data
            vm.OpportunityPipelineData = await oppQuery.GroupBy(o => o.Stage)
                .Select(g => new { Stage = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Stage, x => x.Count);

            // Monthly Sales Data (last 6 months)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            vm.MonthlySalesData = await oppQuery
                .Where(o => o.Stage == "Won" && o.CreatedDate >= sixMonthsAgo)
                .GroupBy(o => new { o.CreatedDate.Year, o.CreatedDate.Month })
                .Select(g => new { Key = g.Key.Year + "-" + g.Key.Month.ToString("D2"), Total = g.Sum(o => o.Amount) })
                .ToDictionaryAsync(x => x.Key, x => x.Total);

            return vm;
        }
    }
}

