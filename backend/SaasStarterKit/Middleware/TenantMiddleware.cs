using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;
using SaasStarterKit.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SaasStarterKit.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService, UserManager<ApplicationUser> userManager, ICacheService cacheService)
        {
            // check token blacklist first
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jti))
            {
                var isBlacklisted = await cacheService.IsTokenBlacklistedAsync(jti, context.RequestAborted);
                if (isBlacklisted)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        title = "Unauthorized",
                        status = 401,
                        detail = "Token has been revoked."
                    });
                    return;
                }
            }

            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;

            if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                tenantService.SetCurrentTenantId(tenantId);
            }

            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null && !user.IsActive)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        title = "Unauthorized",
                        status = 401,
                        detail = "Your account has been deactivated."
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}
