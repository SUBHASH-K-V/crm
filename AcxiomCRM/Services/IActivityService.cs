using AcxiomCRM.Models;

namespace AcxiomCRM.Services
{
    public interface IActivityService
    {
        Task<IEnumerable<Activity>> GetAllAsync(string? userId = null, string? role = null);
        Task<Activity?> GetByIdAsync(int id);
        Task<Activity> CreateAsync(Activity activity);
        Task UpdateAsync(Activity activity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Activity>> SearchAsync(string? type, DateTime? date, string? status, string? assignedTo, string? userId = null, string? role = null);
    }
}

