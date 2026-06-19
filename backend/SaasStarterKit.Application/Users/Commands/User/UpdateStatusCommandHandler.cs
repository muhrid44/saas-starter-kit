using MediatR;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Users.Commands.User
{
    public record UpdateStatusCommand(Guid UserId, bool IsActive) : IRequest;

    public class UpdateStatusCommandHandler : IRequestHandler<UpdateStatusCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly ICacheService _cacheService;
        private readonly IDbTransactionService _dbTransactionService;
        private readonly IAuditLogRepository _auditLogRepository;

        public UpdateStatusCommandHandler(UserManager<ApplicationUser> userManager, ITenantService tenantService, ICacheService cacheService, IDbTransactionService dbTransactionService, IAuditLogRepository auditLogRepository)
        {
            _userManager = userManager;
            _tenantService = tenantService;
            _cacheService = cacheService;
            _dbTransactionService = dbTransactionService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null || user.TenantId != _tenantService.GetCurrentTenantId())
                throw new InvalidOperationException("User not found.");

            user.IsActive = request.IsActive;
            user.ModifiedDate = DateTime.UtcNow;

            string status = request.IsActive ? "activated" : "deactivated";

            var cacheKey = $"users:tenant:{_tenantService.GetCurrentTenantId()}";

            await _cacheService.RemoveAsync(cacheKey);

            await using var transaction = await _dbTransactionService.BeginTransactionAsync(cancellationToken);

            try
            {
                await _userManager.UpdateAsync(user);

                await _auditLogRepository.LogAsync("Update Status", $"{user.FullName}'s just {status}", cancellationToken);

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