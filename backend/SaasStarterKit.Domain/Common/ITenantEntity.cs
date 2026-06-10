using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Domain.Common
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
    }
}
