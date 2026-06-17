using SaasStarterKit.Application.Common.Interfaces;

namespace SaasStarterKit.Application.Common.Services
{
    public class TenantService : ITenantService
    {
        private Guid _currentTenantId;

        public Guid GetCurrentTenantId() => _currentTenantId;

        public void SetCurrentTenantId(Guid tenantId)
        {
            _currentTenantId = tenantId;
        }
    }
}
