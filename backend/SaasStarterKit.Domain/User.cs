using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
