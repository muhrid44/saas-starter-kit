using MediatR;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SaasStarterKit.Application.Users.Queries.GetUsers
{
    public record UserProfileResponse(Guid Id, string Email, string FullName, bool IsActive, Guid TenantId, List<string> Roles);

    public record UserProfileRequest : IRequest<UserProfileResponse>;

    public class GetUserProfileCommandHandler : IRequestHandler<UserProfileRequest, UserProfileResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;

        public GetUserProfileCommandHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, ICacheService cacheService)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
        }

        public async Task<UserProfileResponse> Handle(UserProfileRequest request, CancellationToken cancellationToken)
        {
            var email = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                throw new UnauthorizedAccessException("User not found.");

            var cacheKey = $"me:{email}";
            var cached = await _cacheService.GetAsync<UserProfileResponse>(cacheKey, cancellationToken);
            if (cached != null) return cached;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account has been deactivated.");

            var roles = await _userManager.GetRolesAsync(user);

            var response = new UserProfileResponse(
                user.Id,
                user.Email!,
                user.FullName,
                user.IsActive,
                user.TenantId,
                roles.ToList()
            );

            await _cacheService.SetAsync(cacheKey, response, cancellationToken: cancellationToken);

            return response;
        }
    }
}