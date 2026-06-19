using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SaasStarterKit.Application.Users.Commands.User
{
    public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest;

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;
        private readonly IDbTransactionService _dbTransactionService;
        private readonly IAuditLogRepository _auditLogRepository;

        public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, ICacheService cacheService, IDbTransactionService dbTransactionService, IAuditLogRepository auditLogRepository)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
            _dbTransactionService = dbTransactionService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var email = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                throw new UnauthorizedAccessException("User not found.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var isSamePassword = await _userManager.CheckPasswordAsync(user, request.NewPassword);

            if (isSamePassword)
            {
                throw new InvalidOperationException("The new password must be different from the current password.");
            }

            await using var transaction = await _dbTransactionService.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                user.ModifiedDate = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                // only blacklist AFTER successful password change
                var jti = _httpContextAccessor.HttpContext?.User
                    .FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (!string.IsNullOrEmpty(jti))
                    await _cacheService.BlacklistTokenAsync(jti, TimeSpan.FromMinutes(60), cancellationToken);

                await _auditLogRepository.LogAsync("Change Password", $"{user.FullName} has changed the password", cancellationToken);

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