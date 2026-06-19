using SaasStarterKit.Application.AuditLogs.Queries;

namespace SaasStarterKit.Application.Common.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLogDto>> GetAuditLogsAsync(Guid tenantId, CancellationToken cancellationToken);
        Task<int> GetCountAuditLogsEvent(Guid tenantId, CancellationToken cancellationToken);
        Task LogAsync(string eventName, string description, CancellationToken cancellationToken = default);
    }
}
