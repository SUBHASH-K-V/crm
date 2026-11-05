using AcxiomCRM.Models;

namespace AcxiomCRM.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync(string? userId = null, string? role = null);
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer> CreateAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
        Task<bool> IsDuplicateEmailAsync(string email, int? excludeId = null);
        Task<bool> IsDuplicatePhoneAsync(string phone, int? excludeId = null);
        Task<IEnumerable<Customer>> SearchAsync(string? name, string? email, string? phone, string? company, string? userId = null, string? role = null);
    }
}

