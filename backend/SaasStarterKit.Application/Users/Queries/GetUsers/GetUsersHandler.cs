using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Users.Queries.GetUsers
{

    public record UserDto(Guid Id, string Email, string FullName, bool IsActive);

    public record UsersDto : IRequest<List<UserDto>>;

    public class GetUsersHandler : IRequestHandler<UsersDto, List<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly ICacheService _cacheService;

        public GetUsersHandler(UserManager<ApplicationUser> userManager, ITenantService tenantService , ICacheService cacheService)
        {
            _userManager = userManager;
            _tenantService = tenantService;
            _cacheService = cacheService;
        }
        public async Task<List<UserDto>> Handle(UsersDto request, CancellationToken cancellationToken)
        {
            var currentTenantId = _tenantService.GetCurrentTenantId();
            var cacheKey = $"users:tenant:{currentTenantId}";

            // try cache first
            var cached = await _cacheService.GetAsync<List<UserDto>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                Console.WriteLine($"Cache HIT for {cacheKey}");
                return cached;
            }

            Console.WriteLine($"Cache MISS for {cacheKey}");

            var users = await _userManager.Users
                        .Where(u => u.TenantId == currentTenantId)
                        .Select(u => new UserDto(u.Id, u.Email, u.FullName, u.IsActive))
                        .ToListAsync(cancellationToken);

            // store in cache
            await _cacheService.SetAsync(cacheKey, users, cancellationToken: cancellationToken);

            return users;
        }
    }
}
