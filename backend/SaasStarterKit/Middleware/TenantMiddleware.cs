using SaasStarterKit.Application.Common.Interfaces;

namespace SaasStarterKit.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;

            if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                tenantService.SetCurrentTenantId(tenantId);
            }

            await _next(context);
        }
    }
}
