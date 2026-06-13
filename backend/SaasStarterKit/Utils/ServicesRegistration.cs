using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Common.Jobs;
using SaasStarterKit.Application.Common.Services;
using SaasStarterKit.Infrastructure.Repositories;
using SaasStarterKit.Infrastructure.Services;

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
            builder.Services.AddScoped<EmailJob>();
            builder.Services.AddScoped<ICacheService, CacheService>();
        }
    }
}
