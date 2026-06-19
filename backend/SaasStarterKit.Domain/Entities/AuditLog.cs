using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string EventName { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string ChangedBy { get; set; } = default!;
        public DateTime ChangedDate { get; set; }
        public Guid? TenantId { get; set; }
    }
}
