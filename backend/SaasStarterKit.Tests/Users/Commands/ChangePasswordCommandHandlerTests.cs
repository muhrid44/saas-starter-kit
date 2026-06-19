using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Users.Commands.User;
using SaasStarterKit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace SaasStarterKit.Tests.Users.Commands
{
    [TestFixture]
    public class ChangePasswordCommandHandlerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<IDbTransactionService> _dbTransactionServiceMock;
        private Mock<IAuditLogRepository> _auditLogRepositoryMock;
        private ChangePasswordCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _cacheServiceMock = new Mock<ICacheService>();
            _dbTransactionServiceMock = new Mock<IDbTransactionService>();
            _auditLogRepositoryMock = new Mock<IAuditLogRepository>();

            // Setup default mocks
            var mockTransaction = new Mock<IDbContextTransaction>();
            mockTransaction
                .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            mockTransaction
                .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _dbTransactionServiceMock
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            _cacheServiceMock
                .Setup(x => x.BlacklistTokenAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _auditLogRepositoryMock
                .Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _handler = new ChangePasswordCommandHandler(
                _userManagerMock.Object,
                _httpContextAccessorMock.Object,
                _cacheServiceMock.Object,
                _dbTransactionServiceMock.Object,
                _auditLogRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_ValidPasswordChange_SuccessfullyChangesPassword()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FullName = "Test User"
            };

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "test@test.com"),
                new Claim(JwtRegisteredClaimNames.Jti, "test-jti-token")
            }));

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "NewPassword@123"))
                .ReturnsAsync(false);

            var successResult = IdentityResult.Success;
            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(user, "OldPassword@123", "NewPassword@123"))
                .ReturnsAsync(successResult);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _handler.Handle(
                new ChangePasswordCommand("OldPassword@123", "NewPassword@123"),
                CancellationToken.None);

            // Assert
            _userManagerMock.Verify(x => x.ChangePasswordAsync(user, "OldPassword@123", "NewPassword@123"), Times.Once);
            _cacheServiceMock.Verify(x => x.BlacklistTokenAsync("test-jti-token", TimeSpan.FromMinutes(60), It.IsAny<CancellationToken>()), Times.Once);
            _auditLogRepositoryMock.Verify(x => x.LogAsync("Change Password", $"{user.FullName} has changed the password", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_NoEmailInContext_ThrowsUnauthorizedException()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _handler.Handle(
                    new ChangePasswordCommand("OldPassword@123", "NewPassword@123"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "notfound@test.com")
            }));

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("notfound@test.com"))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _handler.Handle(
                    new ChangePasswordCommand("OldPassword@123", "NewPassword@123"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_SamePassword_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FullName = "Test User"
            };

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "test@test.com")
            }));

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "SamePassword@123"))
                .ReturnsAsync(true);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(
                    new ChangePasswordCommand("SamePassword@123", "SamePassword@123"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_PasswordChangeFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                FullName = "Test User"
            };

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "test@test.com")
            }));

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("test@test.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "NewPassword@123"))
                .ReturnsAsync(false);

            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Invalid password" });
            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(user, "OldPassword@123", "NewPassword@123"))
                .ReturnsAsync(failedResult);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(
                    new ChangePasswordCommand("OldPassword@123", "NewPassword@123"),
                    CancellationToken.None));
        }
    }
}
