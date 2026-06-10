using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Common.Services;
using SaasStarterKit.Infrastructure.Repositories;

namespace SaasStarterKit.API.Utils
{
    public static class ServicesRegistration
    {
        public static void Register(WebApplicationBuilder builder)
        {
            // Register application services
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<ITenantService, TenantService>();
        }
    }
}
