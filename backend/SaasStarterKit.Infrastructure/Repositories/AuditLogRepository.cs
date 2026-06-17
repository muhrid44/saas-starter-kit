using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaasStarterKit.Application.AuditLogs.Queries;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;
using System.Text.Json;

namespace SaasStarterKit.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ITenantService _tenantService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogRepository(ITenantService tenantService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _tenantService = tenantService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<AuditLogDto>> GetAuditLogsAsync(Guid tenantId, CancellationToken cancellationToken)
        {
            var auditLogs = await _context.AuditLogs
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.ChangedAt)
                .Select(a => new AuditLogDto(
                    a.Id,
                    a.EntityName,
                    a.Action,
                    a.OldValues,
                    a.NewValues,
                    a.ChangedBy,
                    a.ChangedAt
                ))
                .ToListAsync(cancellationToken);

            return auditLogs;
        }
    }
}
