using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Infrastructure.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly ApplicationDbContext _context;

        public TenantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsSlugTakenAsync(string slug, CancellationToken cancellationToken)
        {
            return await _context.Tenants
                .AnyAsync(t => t.Slug == slug, cancellationToken);
        }

        public async Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken)
        {
            await _context.Tenants.AddAsync(tenant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return tenant;
        }
    }
}
