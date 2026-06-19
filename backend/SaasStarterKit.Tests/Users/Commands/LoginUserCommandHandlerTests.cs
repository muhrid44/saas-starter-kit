using Microsoft.AspNetCore.Identity;
using Moq;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Users.Commands.Login;
using SaasStarterKit.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace SaasStarterKit.Tests.Users.Commands
{
    [TestFixture]
    public class LoginUserCommandHandlerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<IJwtService> _jwtServiceMock;
        private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private Mock<IAuditLogRepository> _auditLogRepository;
        private Mock<IDbTransactionService> _dbTransactionService;

        private LoginUserCommandHandler _handler;
        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _jwtServiceMock = new Mock<IJwtService>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _auditLogRepository = new Mock<IAuditLogRepository>();
            _dbTransactionService = new Mock<IDbTransactionService>();

            // Setup mock for transaction
            var mockTransaction = new Mock<IDbContextTransaction>();
            mockTransaction
                .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            mockTransaction
                .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _dbTransactionService
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            // Setup default mocks for repositories
            _refreshTokenRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _auditLogRepository
                .Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _handler = new LoginUserCommandHandler(
                _userManagerMock.Object,
                _jwtServiceMock.Object,
                _refreshTokenRepositoryMock.Object,
                _auditLogRepository.Object,
                _dbTransactionService.Object);
        }

        [Test]
        public async Task Handle_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FullName = "Test User",
                IsActive = true
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "Test@123456"))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            _jwtServiceMock
                .Setup(x => x.GenerateToken(user, "Admin"))
                .Returns("fake-access-token");

            _jwtServiceMock
                .Setup(x => x.GenerateRefreshToken(user.Id))
                .Returns(new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    Token = "fake-refresh-token",
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedDate = DateTime.UtcNow,
                    IsRevoked = false
                });

            // Act
            var result = await _handler.Handle(
                new LoginUserCommand("test@test.com", "Test@123456"),
                CancellationToken.None);

            // Assert
            Assert.That(result.AccessToken, Is.EqualTo("fake-access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("fake-refresh-token"));
        }

        [Test]
        public async Task Handle_InvalidCredentials_ThrowsUnauthorizedException()
        {
            // Arrange
            _userManagerMock
                .Setup(x => x.FindByEmailAsync("wrong@test.com"))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _handler.Handle(
                    new LoginUserCommand("wrong@test.com", "wrongpassword"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_WrongPassword_ThrowsUnauthorizedException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FullName = "Test User"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "wrongpassword"))
                .ReturnsAsync(false);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _handler.Handle(
                    new LoginUserCommand("test@test.com", "wrongpassword"),
                    CancellationToken.None));
        }
    }
}
