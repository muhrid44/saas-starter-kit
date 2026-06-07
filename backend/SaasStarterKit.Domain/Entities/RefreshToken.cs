using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public required string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
