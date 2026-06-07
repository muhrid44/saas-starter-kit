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

        public LoginUserCommandHandler(UserManager<ApplicationUser> userManager, IJwtService jwtService, IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var accessToken = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

            return new AuthResponse(accessToken, refreshToken.Token);
        }
    }
}
