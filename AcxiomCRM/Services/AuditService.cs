using AcxiomCRM.Data;
using AcxiomCRM.Models;

namespace AcxiomCRM.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string? userId, string action, string entityName, string? recordId = null, string? oldValue = null, string? newValue = null, string? ipAddress = null)
        {
            var audit = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                RecordId = recordId,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = ipAddress,
                CreatedDate = DateTime.UtcNow
            };
            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}

