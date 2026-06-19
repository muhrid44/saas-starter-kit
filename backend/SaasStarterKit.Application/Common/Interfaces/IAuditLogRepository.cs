using SaasStarterKit.Application.AuditLogs.Queries;

namespace SaasStarterKit.Application.Common.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<(List<AuditLogDto> Items, int TotalCount)> GetAuditLogsAsync(
            Guid tenantId,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
        Task<int> GetCountAuditLogsEvent(Guid tenantId, CancellationToken cancellationToken);
        Task LogAsync(string eventName, string description, CancellationToken cancellationToken = default, Guid? tenantGuid = null);
    }
}
