using AcxiomCRM.Models;

namespace AcxiomCRM.Services
{
    public interface IReportService
    {
        Task<IEnumerable<Customer>> GetCustomerReportAsync(string? status = null, string? ownerId = null);
        Task<IEnumerable<Lead>> GetLeadReportAsync(string? source = null, string? status = null, string? ownerId = null);
        Task<IEnumerable<FollowUp>> GetFollowUpReportAsync(string? status = null, string? assignedTo = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<Opportunity>> GetOpportunityReportAsync(string? stage = null, string? status = null, string? ownerId = null);
        Task<Dictionary<string, decimal>> GetPipelineReportAsync();
        Task<IEnumerable<AuditLog>> GetAuditReportAsync(string? userId = null, string? action = null, string? module = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}

