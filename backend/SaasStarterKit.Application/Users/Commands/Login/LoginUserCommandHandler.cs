using MediatR;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Users.Commands.Login
{
    public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponse>;
    public record AuthResponse(string AccessToken, string RefreshToken);
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IDbTransactionService _dbTransactionService;

        public LoginUserCommandHandler(UserManager<ApplicationUser> userManager, IJwtService jwtService, IRefreshTokenRepository refreshTokenRepository, IAuditLogRepository auditLogRepository, IDbTransactionService dbTransactionService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _auditLogRepository = auditLogRepository;
            _dbTransactionService = dbTransactionService;
        }

        public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account has been deactivated.");

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtService.GenerateToken(user, roles.FirstOrDefault() ?? "User"); 
            var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

            await using var transaction = await _dbTransactionService.BeginTransactionAsync(cancellationToken);

            try
            {
                await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
                await _auditLogRepository.LogAsync("Login", $"{user.FullName} has logged in", cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }



            return new AuthResponse(accessToken, refreshToken.Token);
        }
    }
}
