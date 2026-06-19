using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Domain.Common;

namespace SaasStarterKit.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>, ITenantEntity
    {
        public required string FullName  { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
