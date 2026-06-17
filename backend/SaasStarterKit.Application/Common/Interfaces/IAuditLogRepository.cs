using SaasStarterKit.Application.AuditLogs.Queries;

namespace SaasStarterKit.Application.Common.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLogDto>> GetAuditLogsAsync(Guid tenantId, CancellationToken cancellationToken);
    }
}
