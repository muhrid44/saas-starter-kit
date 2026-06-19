using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;
using System.Security.Claims;

namespace SaasStarterKit.Application.Users.Commands.User
{
    public record UpdateProfileCommand(string FullName, string Email) : IRequest;

    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDbTransactionService _dbTransactionService;
        private readonly IAuditLogRepository _auditLogRepository;


        public UpdateProfileCommandHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IDbTransactionService dbTransactionService, IAuditLogRepository auditLogRepository)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _dbTransactionService = dbTransactionService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var email = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                throw new UnauthorizedAccessException("User not found.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            user.FullName = request.FullName;

            await using var transaction = await _dbTransactionService.BeginTransactionAsync(cancellationToken);

            try
            {
                user.ModifiedDate = DateTime.UtcNow;

                if (user.Email != request.Email)
                {
                    var setEmailResult = await _userManager.SetEmailAsync(user, request.Email);
                    if (!setEmailResult.Succeeded)
                        throw new InvalidOperationException(string.Join(", ", setEmailResult.Errors.Select(e => e.Description)));

                    await _userManager.SetUserNameAsync(user, request.Email);
                }
                else
                {
                    await _userManager.UpdateAsync(user);
                }

                await _auditLogRepository.LogAsync("Update Profile", $"{user.FullName} just updated their profile", cancellationToken);

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