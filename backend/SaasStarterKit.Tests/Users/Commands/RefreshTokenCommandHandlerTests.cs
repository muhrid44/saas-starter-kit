using Microsoft.AspNetCore.Identity;
using Moq;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Users.Commands.RefreshToken;
using SaasStarterKit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaasStarterKit.Tests.Users.Commands
{
    [TestFixture]
    public class RefreshTokenCommandHandlerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<IJwtService> _jwtServiceMock;
        private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private RefreshTokenCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _jwtServiceMock = new Mock<IJwtService>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

            _handler = new RefreshTokenCommandHandler(
                _refreshTokenRepositoryMock.Object,
                _jwtServiceMock.Object,
                _userManagerMock.Object);
        }

        [Test]
        public async Task Handle_ValidRefreshToken_ReturnsNewAuthResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@test.com",
                FullName = "Test User"
            };

            var existingToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "valid-refresh-token",
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedDate = DateTime.UtcNow,
                IsRevoked = false
            };

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingToken);

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            _jwtServiceMock
                .Setup(x => x.GenerateToken(user, "Admin"))
                .Returns("new-access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken(userId))
                .Returns(new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    Token = "new-refresh-token",
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedDate = DateTime.UtcNow,
                    IsRevoked = false
                });

            // Act
            var result = await _handler.Handle(
                new RefreshTokenCommand("valid-refresh-token"),
                CancellationToken.None);

            // Assert
            Assert.That(result.AccessToken, Is.EqualTo("new-access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("new-refresh-token"));
        }

        [Test]
        public async Task Handle_RevokedToken_ThrowsUnauthorizedException()
        {
            // Arrange
            var existingToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "revoked-token",
                UserId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = true // ← already revoked
            };

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync("revoked-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingToken);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _handler.Handle(
                    new RefreshTokenCommand("revoked-token"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_ExpiredToken_ThrowsUnauthorizedException()
        {
            // Arrange
            var existingToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "expired-token",
                UserId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // ← expired yesterday
                IsRevoked = false
            };

            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync("expired-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingToken);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _handler.Handle(
                    new RefreshTokenCommand("expired-token"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_TokenNotFound_ThrowsUnauthorizedException()
        {
            // Arrange
            _refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync("nonexistent-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken)null);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _handler.Handle(
                    new RefreshTokenCommand("nonexistent-token"),
                    CancellationToken.None));
        }
    }
}
