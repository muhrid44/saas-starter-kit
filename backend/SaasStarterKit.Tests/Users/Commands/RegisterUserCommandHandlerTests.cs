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
    public class RegisterUserCommandHandlerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<ITenantService> _tenantServiceMock;
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<IDbTransactionService> _dbTransactionServiceMock;
        private Mock<IAuditLogRepository> _auditLogRepositoryMock;
        private RegisterUserCommandHandler _handler;

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

            _handler = new RegisterUserCommandHandler(
                _userManagerMock.Object,
                _tenantServiceMock.Object,
                _cacheServiceMock.Object,
                _dbTransactionServiceMock.Object,
                _auditLogRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_ValidRegistration_CreatesNewUser()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("newuser@test.com"))
                .ReturnsAsync((ApplicationUser)null);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantId);

            var createdUser = new ApplicationUser
            {
                Id = userId,
                UserName = "newuser@test.com",
                Email = "newuser@test.com",
                FullName = "New User",
                IsActive = true,
                TenantId = tenantId
            };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password@123"))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, _) => user.Id = userId);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(
                new RegisterUserCommand("newuser@test.com", "New User", "Password@123"),
                CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(userId));
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password@123"), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
            _cacheServiceMock.Verify(x => x.RemoveAsync($"users:tenant:{tenantId}"), Times.Once);
        }

        [Test]
        public async Task Handle_EmailAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "existing@test.com",
                FullName = "Existing User"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("existing@test.com"))
                .ReturnsAsync(existingUser);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(
                    new RegisterUserCommand("existing@test.com", "New User", "Password@123"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_UserCreationFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("newuser@test.com"))
                .ReturnsAsync((ApplicationUser)null);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantId);

            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Password too weak" });
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "weakpass"))
                .ReturnsAsync(failedResult);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(
                    new RegisterUserCommand("newuser@test.com", "New User", "weakpass"),
                    CancellationToken.None));
        }
    }
}
