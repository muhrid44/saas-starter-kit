using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace SaasStarterKit.Application.Users.Commands.User
{
    public record ResetPasswordCommand(Guid UserId, string NewPassword) : IRequest;

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly IDbTransactionService _dbTransactionService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuditLogRepository _auditLogRepository;


        public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager, ITenantService tenantService, IDbTransactionService dbTransactionService, IRefreshTokenRepository refreshTokenRepository, IAuditLogRepository auditLogRepository)
        {
            _userManager = userManager;
            _tenantService = tenantService;
            _dbTransactionService = dbTransactionService;
            _refreshTokenRepository = refreshTokenRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null || user.TenantId != _tenantService.GetCurrentTenantId())
                throw new InvalidOperationException("User not found.");

            var isSamePassword = await _userManager.CheckPasswordAsync(user, request.NewPassword);

            if (isSamePassword)
            {
                throw new InvalidOperationException("The new password must be different from the current password.");
            }

            await using var transaction = await _dbTransactionService.BeginTransactionAsync(cancellationToken);

            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                user.ModifiedDate = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id, cancellationToken);

                await _auditLogRepository.LogAsync("Password Reset", $"{user.FullName}'s password has been reset", cancellationToken);

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