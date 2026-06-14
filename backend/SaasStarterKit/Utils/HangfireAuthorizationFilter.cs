using Hangfire.Dashboard;

namespace SaasStarterKit.API.Utils
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // allow all in development
            return true;
        }
    }
}
