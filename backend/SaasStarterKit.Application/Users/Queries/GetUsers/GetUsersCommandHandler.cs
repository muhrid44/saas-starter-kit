using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Users.Queries.GetUsers
{

    public record UserDto(Guid Id, string Email, string FullName, bool IsActive, DateTime CreatedDate, DateTime? ModifiedDate, List<string> Roles);

    public record UsersDto : IRequest<List<UserDto>>;

    public class GetUsersCommandHandler : IRequestHandler<UsersDto, List<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly ICacheService _cacheService;

        public GetUsersCommandHandler(UserManager<ApplicationUser> userManager, ITenantService tenantService , ICacheService cacheService)
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

            // fetch users first
            var users = await _userManager.Users
                        .Where(u => u.TenantId == currentTenantId)
                        .ToListAsync(cancellationToken);

            // fetch all roles in parallel
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto(
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.IsActive,
                    user.CreatedDate,
                    user.ModifiedDate,
                    roles.ToList()
                ));
            }

            // store in cache
            await _cacheService.SetAsync(cacheKey, userDtos, cancellationToken: cancellationToken);

            return userDtos.OrderByDescending(res => res.ModifiedDate).ToList();
        }
    }
}
