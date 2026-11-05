using AcxiomCRM.Models;

namespace AcxiomCRM.Services
{
    public interface IFollowUpService
    {
        Task<IEnumerable<FollowUp>> GetAllAsync(string? userId = null, string? role = null);
        Task<FollowUp?> GetByIdAsync(int id);
        Task<FollowUp> CreateAsync(FollowUp followUp);
        Task UpdateAsync(FollowUp followUp);
        Task<IEnumerable<FollowUp>> GetUpcomingAsync(string? userId = null, string? role = null);
        Task<IEnumerable<FollowUp>> GetOverdueAsync(string? userId = null, string? role = null);
        Task<IEnumerable<FollowUp>> SearchAsync(DateTime? date, string? status, string? assignedTo, string? userId = null, string? role = null);
    }
}

