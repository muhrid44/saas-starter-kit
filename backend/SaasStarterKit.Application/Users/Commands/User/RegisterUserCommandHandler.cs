using MediatR;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Users.Commands.User
{
    public record RegisterUserCommand(string Email, string FullName, string Password) : IRequest<Guid>;

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly ICacheService _cacheService;
        private readonly IDbTransactionService _dbTransactionService;
        private readonly IAuditLogRepository _auditLogRepository;

        public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, ITenantService tenantService, ICacheService cacheService, IDbTransactionService dbTransactionService, IAuditLogRepository auditLogRepository)
        {
            _userManager = userManager;
            _tenantService = tenantService;
            _cacheService = cacheService;
            _dbTransactionService = dbTransactionService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already in use.");

            var tenantId = _tenantService.GetCurrentTenantId();

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                TenantId = tenantId
            };

            await using var transaction = await _dbTransactionService.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(", ", result.Errors.Select(e => e.Description)));

                await _userManager.AddToRoleAsync(user, "User");

                var cacheKey = $"users:tenant:{tenantId}";

                await _cacheService.RemoveAsync(cacheKey);

                await _auditLogRepository.LogAsync("User Registration", $"New user has been registered : {user.FullName}", cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return user.Id;
            } catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}