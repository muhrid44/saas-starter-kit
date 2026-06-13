using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public required string EntityName { get; set; }
        public required string Action { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public Guid? TenantId { get; set; }
    }
}
