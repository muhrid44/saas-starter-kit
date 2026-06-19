using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SaasStarterKit.Application.Users.Commands.User
{
    public record DeleteUserCommand(Guid UserId) : IRequest;

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly IDbTransactionService _dbTransactionService;
        private readonly ICacheService _cacheService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRefreshTokenRepository _refreshTokenRepository;


        public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager, ITenantService tenantService, IDbTransactionService dbTransactionService, ICacheService cacheService, IAuditLogRepository auditLogRepository, IHttpContextAccessor httpContextAccessor, IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _tenantService = tenantService;
            _dbTransactionService = dbTransactionService;
            _cacheService = cacheService;
            _auditLogRepository = auditLogRepository;
            _httpContextAccessor = httpContextAccessor;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null || user.TenantId != _tenantService.GetCurrentTenantId())
                throw new InvalidOperationException("User not found.");

            await using var transaction = await _dbTransactionService.BeginTransactionAsync(cancellationToken);

            try
            {
                await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id, cancellationToken);

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await _auditLogRepository.LogAsync(
                    "User Deleted",
                    $"Deleted user '{user.FullName}' ({user.Email})",
                    cancellationToken);

                var cacheKey = $"users:tenant:{user.TenantId}";

                await _cacheService.RemoveAsync(cacheKey);

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
