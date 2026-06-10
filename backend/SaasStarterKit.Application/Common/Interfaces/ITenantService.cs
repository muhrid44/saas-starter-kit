using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Application.Common.Interfaces
{
    public interface ITenantService
    {
        Guid GetCurrentTenantId();
        void SetCurrentTenantId(Guid tenantId);
    }
}
