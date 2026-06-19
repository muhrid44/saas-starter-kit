using MediatR;
using SaasStarterKit.Application.Common.Interfaces;

namespace SaasStarterKit.Application.AuditLogs.Queries
{
    public record AuditLogDto(
        Guid Id,
        string EventName,
        string Description,
        string? ChangedBy,
        DateTime ChangedDate,
        Guid? TenantId
    );

    public record PaginatedResult<T>(
        List<T> Items,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages
    );

    public record GetAuditLogsQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedResult<AuditLogDto>>;

    public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, PaginatedResult<AuditLogDto>>
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ITenantService _tenantService;

        public GetAuditLogsHandler(IAuditLogRepository auditLogRepository, ITenantService tenantService)
        {
            _auditLogRepository = auditLogRepository;
            _tenantService = tenantService;
        }

        public async Task<PaginatedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var currentTenantId = _tenantService.GetCurrentTenantId();

            var (items, totalCount) = await _auditLogRepository.GetAuditLogsAsync(
                currentTenantId,
                request.Page,
                request.PageSize,
                cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new PaginatedResult<AuditLogDto>(
                items,
                totalCount,
                request.Page,
                request.PageSize,
                totalPages
            );
        }
    }
}