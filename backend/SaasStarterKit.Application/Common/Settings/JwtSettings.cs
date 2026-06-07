using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Application.Common.Settings
{
    public class JwtSettings
    {
        public required string SecretKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public int ExpiryMinutes { get; set; }
    }
}
