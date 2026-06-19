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
    public class UpdateStatusCommandHandlerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<ITenantService> _tenantServiceMock;
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<IDbTransactionService> _dbTransactionServiceMock;
        private Mock<IAuditLogRepository> _auditLogRepositoryMock;
        private UpdateStatusCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _tenantServiceMock = new Mock<ITenantService>();
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
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _auditLogRepositoryMock
                .Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _handler = new UpdateStatusCommandHandler(
                _userManagerMock.Object,
                _tenantServiceMock.Object,
                _cacheServiceMock.Object,
                _dbTransactionServiceMock.Object,
                _auditLogRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_DeactivateUser_SuccessfullyDeactivates()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@test.com",
                FullName = "Test User",
                IsActive = true,
                TenantId = tenantId
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantId);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _handler.Handle(
                new UpdateStatusCommand(userId, false),
                CancellationToken.None);

            // Assert
            Assert.That(user.IsActive, Is.False);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"users:tenant:{tenantId}"), Times.Once);
            _auditLogRepositoryMock.Verify(x => x.LogAsync("Update Status", $"{user.FullName}'s just deactivated", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ActivateUser_SuccessfullyActivates()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@test.com",
                FullName = "Test User",
                IsActive = false,
                TenantId = tenantId
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantId);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _handler.Handle(
                new UpdateStatusCommand(userId, true),
                CancellationToken.None);

            // Assert
            Assert.That(user.IsActive, Is.True);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"users:tenant:{tenantId}"), Times.Once);
            _auditLogRepositoryMock.Verify(x => x.LogAsync("Update Status", $"{user.FullName}'s just activated", It.IsAny<CancellationToken>()), Times.Once);
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
                    new UpdateStatusCommand(userId, false),
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
                IsActive = true,
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
                    new UpdateStatusCommand(userId, false),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_UpdateThrowsException_RollsBackTransaction()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@test.com",
                FullName = "Test User",
                IsActive = true,
                TenantId = tenantId
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantId);

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

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ThrowsAsync(new Exception("Update failed"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _handler.Handle(
                    new UpdateStatusCommand(userId, false),
                    CancellationToken.None));
        }
    }
}
