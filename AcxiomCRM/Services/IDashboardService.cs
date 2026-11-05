using AcxiomCRM.ViewModels;

namespace AcxiomCRM.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(string? userId = null, string? role = null);
    }
}

