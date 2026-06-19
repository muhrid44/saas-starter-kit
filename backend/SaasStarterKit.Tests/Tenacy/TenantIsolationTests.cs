using Microsoft.AspNetCore.Identity;
using Moq;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Users.Queries.GetUsers;
using SaasStarterKit.Domain.Entities;
using MockQueryable.Moq;
using MockQueryable;

namespace SaasStarterKit.Tests.Tenacy
{
    [TestFixture]
    public class TenantIsolationTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<ITenantService> _tenantServiceMock;
        private Mock<ICacheService> _cacheServiceMock;
        private GetUsersCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _tenantServiceMock = new Mock<ITenantService>();

            _cacheServiceMock = new Mock<ICacheService>();

            // Mock GetRolesAsync to return empty list by default
            _userManagerMock
                .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>());

            _handler = new GetUsersCommandHandler(
                _userManagerMock.Object,
                _tenantServiceMock.Object,
                _cacheServiceMock.Object);
        }

        [Test]
        public async Task GetUsers_OnlyReturnsTenantAUsers_WhenLoggedInAsTenantA()
        {
            // Arrange
            var tenantAId = Guid.NewGuid();
            var tenantBId = Guid.NewGuid();

            var allUsers = new List<ApplicationUser>
        {
            new() { Id = Guid.NewGuid(), Email = "userA1@test.com", FullName = "User A1", TenantId = tenantAId },
            new() { Id = Guid.NewGuid(), Email = "userA2@test.com", FullName = "User A2", TenantId = tenantAId },
            new() { Id = Guid.NewGuid(), Email = "userB1@test.com", FullName = "User B1", TenantId = tenantBId },
        }.BuildMock();

            _userManagerMock
                .Setup(x => x.Users)
                .Returns(allUsers);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantAId);

            var result = await _handler.Handle(new UsersDto(), CancellationToken.None);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(u => u.Email.Contains("userA")), Is.True);
            Assert.That(result.Any(u => u.Email == "userB1@test.com"), Is.False);
        }

        [Test]
        public async Task GetUsers_OnlyReturnsTenantBUsers_WhenLoggedInAsTenantB()
        {
            // Arrange
            var tenantAId = Guid.NewGuid();
            var tenantBId = Guid.NewGuid();

            var allUsers = new List<ApplicationUser>
        {
            new() { Id = Guid.NewGuid(), Email = "userA1@test.com", FullName = "User A1", TenantId = tenantAId },
            new() { Id = Guid.NewGuid(), Email = "userB1@test.com", FullName = "User B1", TenantId = tenantBId },
            new() { Id = Guid.NewGuid(), Email = "userB2@test.com", FullName = "User B2", TenantId = tenantBId },
        }.BuildMock();

            _userManagerMock
                .Setup(x => x.Users)
                .Returns(allUsers);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(tenantBId);

            // Act
            var result = await _handler.Handle(new UsersDto(), CancellationToken.None);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(u => u.Email.Contains("userB")), Is.True);
            Assert.That(result.Any(u => u.Email == "userA1@test.com"), Is.False);
        }

        [Test]
        public async Task GetUsers_ReturnsEmpty_WhenNoUsersInTenant()
        {
            // Arrange
            var tenantAId = Guid.NewGuid();
            var emptyTenantId = Guid.NewGuid();

            var allUsers = new List<ApplicationUser>
        {
            new() { Id = Guid.NewGuid(), Email = "userA1@test.com", FullName = "User A1", TenantId = tenantAId },
        }.BuildMock();

            _userManagerMock
                .Setup(x => x.Users)
                .Returns(allUsers);

            _tenantServiceMock
                .Setup(x => x.GetCurrentTenantId())
                .Returns(emptyTenantId);

            // Act
            var result = await _handler.Handle(new UsersDto(), CancellationToken.None);

            // Assert
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}
