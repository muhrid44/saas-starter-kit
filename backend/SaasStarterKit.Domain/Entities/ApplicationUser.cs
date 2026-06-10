using Microsoft.AspNetCore.Identity;

namespace SaasStarterKit.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public required string FullName  { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
