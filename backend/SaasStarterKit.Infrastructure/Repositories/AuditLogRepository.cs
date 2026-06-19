using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SaasStarterKit.Application.AuditLogs.Queries;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;
using System.Security.Claims;

namespace SaasStarterKit.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantService _tenantService;

        public AuditLogRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ITenantService tenantService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _tenantService = tenantService;
        }

        public async Task<(List<AuditLogDto> Items, int TotalCount)> GetAuditLogsAsync(
            Guid tenantId,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = _context.AuditLogs
                .Where(a => a.TenantId == tenantId)
                .OrderByDescending(a => a.ChangedDate);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogDto(
                    a.Id,
                    a.EventName,
                    a.Description,
                    a.ChangedBy,
                    a.ChangedDate,
                    a.TenantId
                ))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        public async Task<int> GetCountAuditLogsEvent(Guid tenantId, CancellationToken cancellationToken)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            return await _context.AuditLogs.CountAsync(
                a => a.TenantId == tenantId &&
                     a.ChangedDate >= cutoffDate,
                cancellationToken);
        }

        public async Task LogAsync(
        string eventName,
        string description,        
        CancellationToken cancellationToken = default,
        Guid? tenantGuid = null)
        {
            var changedBy =
                _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value
                ?? "System";

            var tenantId = tenantGuid != null ? tenantGuid : _tenantService.GetCurrentTenantId();

            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                EventName = eventName,
                Description = description,
                ChangedBy = changedBy,
                ChangedDate = DateTime.UtcNow,
                TenantId = tenantId
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
