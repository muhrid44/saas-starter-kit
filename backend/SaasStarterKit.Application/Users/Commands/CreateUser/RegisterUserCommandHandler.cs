using MediatR;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Jobs;
using SaasStarterKit.Domain.Entities;


namespace SaasStarterKit.Application.Users.Commands.CreateUser
{
    public record RegisterUserCommand(string Email, string FullName, string Password, Guid TenantId) : IRequest<Guid>;
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already in use.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                IsActive = true,
                CreateAt = DateTime.UtcNow,
                TenantId = request.TenantId
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            return user.Id;
        }
    }
}
