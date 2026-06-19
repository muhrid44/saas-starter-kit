using Microsoft.AspNetCore.Identity;
using Moq;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Users.Commands.User;
using SaasStarterKit.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace SaasStarterKit.Tests.Users.Commands
{
    [TestFixture]
    public class ResetPasswordCommandHandlerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<ITenantService> _tenantServiceMock;
        private Mock<IDbTransactionService> _dbTransactionServiceMock;
        private Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private Mock<IAuditLogRepository> _auditLogRepositoryMock;
        private ResetPasswordCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _tenantServiceMock = new Mock<ITenantService>();
            _dbTransactionServiceMock = new Mock<IDbTransactionService>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
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

            _refreshTokenRepositoryMock
                .Setup(x => x.RevokeAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _auditLogRepositoryMock
                .Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _handler = new ResetPasswordCommandHandler(
                _userManagerMock.Object,
                _tenantServiceMock.Object,
                _dbTransactionServiceMock.Object,
                _refreshTokenRepositoryMock.Object,
                _auditLogRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_ValidPasswordReset_SuccessfullyResetsPassword()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@test.com",
                FullName = "Test User",
                TenantId = tenantId
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantId);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "NewPassword@123"))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, "reset-token", "NewPassword@123"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _handler.Handle(
                new ResetPasswordCommand(userId, "NewPassword@123"),
                CancellationToken.None);

            // Assert
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, "reset-token", "NewPassword@123"), Times.Once);
            _refreshTokenRepositoryMock.Verify(x => x.RevokeAllByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            _auditLogRepositoryMock.Verify(x => x.LogAsync("Password Reset", $"{user.FullName}'s password has been reset", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(
                    new ResetPasswordCommand(userId, "NewPassword@123"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_UserFromDifferentTenant_ThrowsInvalidOperationException()
        {
            // Arrange
            var currentTenantId = Guid.NewGuid();
            var differentTenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@test.com",
                FullName = "Test User",
                TenantId = differentTenantId
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(currentTenantId);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(
                    new ResetPasswordCommand(userId, "NewPassword@123"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_SamePassword_ThrowsInvalidOperationException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@test.com",
                FullName = "Test User",
                TenantId = tenantId
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantId);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "SamePassword@123"))
                .ReturnsAsync(true);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(
                    new ResetPasswordCommand(userId, "SamePassword@123"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_PasswordResetFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@test.com",
                FullName = "Test User",
                TenantId = tenantId
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantId);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "NewPassword@123"))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Invalid reset token" });
            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, "reset-token", "NewPassword@123"))
                .ReturnsAsync(failedResult);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(
                    new ResetPasswordCommand(userId, "NewPassword@123"),
                    CancellationToken.None));
        }
    }
}
