using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Application.Users.Commands.User;
using SaasStarterKit.Domain.Entities;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace SaasStarterKit.Tests.Users.Commands
{
    [TestFixture]
    public class UpdateProfileCommandHandlerTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IDbTransactionService> _dbTransactionServiceMock;
        private Mock<IAuditLogRepository> _auditLogRepositoryMock;
        private UpdateProfileCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
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

            _auditLogRepositoryMock
                .Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _handler = new UpdateProfileCommandHandler(
                _userManagerMock.Object,
                _httpContextAccessorMock.Object,
                _dbTransactionServiceMock.Object,
                _auditLogRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_UpdateProfileWithSameEmail_SuccessfullyUpdatesProfile()
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
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _handler.Handle(
                new UpdateProfileCommand("Updated Name", "test@test.com"),
                CancellationToken.None);

            // Assert
            Assert.That(user.FullName, Is.EqualTo("Updated Name"));
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
            _auditLogRepositoryMock.Verify(x => x.LogAsync("Update Profile", $"{user.FullName} just updated their profile", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_UpdateProfileWithDifferentEmail_SuccessfullyUpdatesEmailAndProfile()
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
                .Setup(x => x.SetEmailAsync(user, "newemail@test.com"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.SetUserNameAsync(user, "newemail@test.com"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _handler.Handle(
                new UpdateProfileCommand("Updated Name", "newemail@test.com"),
                CancellationToken.None);

            // Assert
            Assert.That(user.FullName, Is.EqualTo("Updated Name"));
            _userManagerMock.Verify(x => x.SetEmailAsync(user, "newemail@test.com"), Times.Once);
            _userManagerMock.Verify(x => x.SetUserNameAsync(user, "newemail@test.com"), Times.Once);
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
                    new UpdateProfileCommand("Updated Name", "newemail@test.com"),
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
                    new UpdateProfileCommand("Updated Name", "newemail@test.com"),
                    CancellationToken.None));
        }

        [Test]
        public async Task Handle_SetEmailFails_ThrowsInvalidOperationException()
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

            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Email already in use" });
            _userManagerMock
                .Setup(x => x.SetEmailAsync(user, "existing@test.com"))
                .ReturnsAsync(failedResult);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _handler.Handle(
                    new UpdateProfileCommand("Updated Name", "existing@test.com"),
                    CancellationToken.None));
        }
    }
}
