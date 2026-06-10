using MediatR;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Users.Commands.Login;
using SaasStarterKit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Application.Users.Commands.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IJwtService jwtService,
            UserManager<ApplicationUser> userManager)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
            _userManager = userManager;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

            if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());

            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            // Revoke old token
            existingToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id);
            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

            return new AuthResponse(newAccessToken, newRefreshToken.Token);
        }
    }
}
