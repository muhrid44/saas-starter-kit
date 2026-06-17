using MediatR;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;


namespace SaasStarterKit.Application.AuditLogs.Queries
{
    public record AuditLogDto(
       Guid Id,
       string EntityName,
       string Action,
       string? OldValues,
       string? NewValues,
       string? ChangedBy,
       DateTime ChangedAt
   );

    public record GetAuditLogsQuery : IRequest<List<AuditLogDto>>;

    public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogDto>>
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ITenantService _tenantService;

        public GetAuditLogsHandler(IAuditLogRepository auditLogRepository, ITenantService tenantService)
        {
            _auditLogRepository = auditLogRepository;
            _tenantService = tenantService;
        }

        public async Task<List<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var currentTenantId = _tenantService.GetCurrentTenantId();

            return await _auditLogRepository.GetAuditLogsAsync(currentTenantId, cancellationToken);
        }
    }
}
