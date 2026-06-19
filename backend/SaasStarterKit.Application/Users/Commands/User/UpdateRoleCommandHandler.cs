using MediatR;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Users.Commands.User
{
    public record UpdateRoleCommand(Guid UserId, string Role) : IRequest;

    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly IDbTransactionService _dbTransactionService;
        private readonly ICacheService _cacheService;
        private readonly IAuditLogRepository _auditLogRepository;

        public UpdateRoleCommandHandler(UserManager<ApplicationUser> userManager, ITenantService tenantService, IDbTransactionService dbTransactionService, ICacheService cacheService, IAuditLogRepository auditLogRepository)
        {
            _userManager = userManager;
            _tenantService = tenantService;
            _dbTransactionService = dbTransactionService;
            _cacheService = cacheService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null || user.TenantId != _tenantService.GetCurrentTenantId())
                throw new InvalidOperationException("User not found.");

            await using var transaction = await _dbTransactionService.BeginTransactionAsync(cancellationToken);

            try
            {
                // remove all existing roles first
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // assign new role
                await _userManager.AddToRoleAsync(user, request.Role);

                user.ModifiedDate = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                var cacheKey = $"users:tenant:{_tenantService.GetCurrentTenantId()}";

                await _cacheService.RemoveAsync(cacheKey);

                await _auditLogRepository.LogAsync("Update Role", $"{user.FullName} just assigned to a new role", cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}