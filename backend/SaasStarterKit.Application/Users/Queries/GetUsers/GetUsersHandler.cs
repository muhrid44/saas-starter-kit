using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

public record UserDto(Guid Id, string Email, string FullName, bool IsActive);

public record UsersDto : IRequest<List<UserDto>>;

namespace SaasStarterKit.Application.Users.Queries.GetUsers
{
    public class GetUsersHandler : IRequestHandler<UsersDto, List<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;

        public GetUsersHandler(UserManager<ApplicationUser> userManager, ITenantService tenantService)
        {
            _userManager = userManager;
            _tenantService = tenantService;
        }
        public async Task<List<UserDto>> Handle(UsersDto request, CancellationToken cancellationToken)
        {
            var currentTenantId = _tenantService.GetCurrentTenantId();

            var users = await _userManager.Users
                        .Where(u => u.TenantId == currentTenantId)
                        .Select(u => new UserDto(u.Id, u.Email, u.FullName, u.IsActive))
                        .ToListAsync(cancellationToken);

            return users;
        }
    }
}
