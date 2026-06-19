using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Users.Queries.Dashboard
{
    public record DashboardInfoDto(int TotalUser, int AuditLogsEvent, int ActiveUser);

    public record DashboardInfo : IRequest<DashboardInfoDto>;

    public class GetDashboardInfoCommandHandler : IRequestHandler<DashboardInfo, DashboardInfoDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ITenantService _tenantService;

        public GetDashboardInfoCommandHandler(UserManager<ApplicationUser> userManager, IAuditLogRepository auditLogRepository, ITenantService tenantService)
        {
            _userManager = userManager;
            _auditLogRepository = auditLogRepository;
            _tenantService = tenantService;
        }

        public async Task<DashboardInfoDto> Handle(DashboardInfo request, CancellationToken cancellationToken)
        {
            Guid tenantId = _tenantService.GetCurrentTenantId();
            int totalUsers = await _userManager.Users.CountAsync(u => u.TenantId == tenantId, cancellationToken);
            int totalUsersACtive = await _userManager.Users.CountAsync(u => u.TenantId == tenantId && u.IsActive, cancellationToken);
            int totalAuditLogs = await _auditLogRepository.GetCountAuditLogsEvent(tenantId, cancellationToken);

            return new DashboardInfoDto(totalUsers, totalAuditLogs, totalUsersACtive);
        }
    }
}
