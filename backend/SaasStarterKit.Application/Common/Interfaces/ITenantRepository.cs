using Microsoft.EntityFrameworkCore.Storage;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Common.Interfaces
{
    public interface ITenantRepository
    {
        Task<bool> IsSlugTakenAsync(string slug, CancellationToken cancellationToken);
        Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken);
    }
}
