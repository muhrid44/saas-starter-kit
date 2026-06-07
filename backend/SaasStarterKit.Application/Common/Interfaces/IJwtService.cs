using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user);
        RefreshToken GenerateRefreshToken(Guid userId);
        Guid? ValidateRefreshToken(string token);
    }
}
