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
            
            var allLeads = await leadQuery.ToListAsync();
            vm.TotalLeads = allLeads.Count;
            vm.OpenLeads = allLeads.Count(l => l.Status != "Converted" && l.Status != "Lost");

            var allOpps = await oppQuery.ToListAsync();
            vm.TotalOpportunities = allOpps.Count;
            vm.OpenOpportunities = allOpps.Count(o => o.Status == "Open");
            vm.WonOpportunities = allOpps.Count(o => o.Stage == "Won");
            vm.LostOpportunities = allOpps.Count(o => o.Stage == "Lost");
            vm.TotalPipelineValue = allOpps.Where(o => o.Status == "Open").Sum(o => o.Amount);

            var allFollowUps = await followUpQuery.ToListAsync();
            vm.PendingFollowUps = allFollowUps.Count(f => f.Status == "Planned" && f.FollowUpDate.Date >= DateTime.Today);
            vm.OverdueFollowUps = allFollowUps.Count(f => f.Status == "Planned" && f.FollowUpDate.Date < DateTime.Today);

            // Lead Status Chart Data
            vm.LeadStatusData = allLeads
                .GroupBy(l => l.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Opportunity Pipeline Chart Data
            vm.OpportunityPipelineData = allOpps
                .GroupBy(o => o.Stage)
                .ToDictionary(g => g.Key, g => g.Count());

            // Monthly Sales Data (last 6 months)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            vm.MonthlySalesData = allOpps
                .Where(o => o.Stage == "Won" && o.CreatedDate >= sixMonthsAgo)
                .GroupBy(o => $"{o.CreatedDate.Year}-{o.CreatedDate.Month:D2}")
                .ToDictionary(g => g.Key, g => g.Sum(o => o.Amount));

            return vm;
        }
    }
}
