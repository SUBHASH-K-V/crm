using AcxiomCRM.Models;

namespace AcxiomCRM.Services
{
    public interface ILeadService
    {
        Task<IEnumerable<Lead>> GetAllAsync(string? userId = null, string? role = null);
        Task<Lead?> GetByIdAsync(int id);
        Task<Lead> CreateAsync(Lead lead);
        Task UpdateAsync(Lead lead);
        Task DeleteAsync(int id);
        Task<Lead?> ConvertToCustomerAsync(int leadId, string userId);
        Task<IEnumerable<Lead>> SearchAsync(string? name, string? company, string? status, string? assignedTo, string? userId = null, string? role = null);
        bool IsValidStatusTransition(string currentStatus, string newStatus);
    }
}

