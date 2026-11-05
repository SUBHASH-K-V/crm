using AcxiomCRM.Models;

namespace AcxiomCRM.Services
{
    public interface IOpportunityService
    {
        Task<IEnumerable<Opportunity>> GetAllAsync(string? userId = null, string? role = null);
        Task<Opportunity?> GetByIdAsync(int id);
        Task<Opportunity> CreateAsync(Opportunity opportunity);
        Task UpdateAsync(Opportunity opportunity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Opportunity>> SearchAsync(string? name, int? customerId, string? stage, string? status, string? userId = null, string? role = null);
    }
}

