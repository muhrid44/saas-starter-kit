using MediatR;
using Microsoft.AspNetCore.Identity;
using SaasStarterKit.Application.Common.Interfaces;
using SaasStarterKit.Domain.Entities;

namespace SaasStarterKit.Application.Users.Commands.Tenant
{
    public record SignupCommand(
        string FullName,
        string Email,
        string Password,
        string TenantName,
        string TenantSlug
    ) : IRequest<SignupResult>;

    public record SignupResult(string AccessToken, string RefreshToken);
    public class SignUpCommandHandler : IRequestHandler<SignupCommand, SignupResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtService _jwtService;
        private readonly ITenantRepository _tenantRepository;
        private readonly IDbTransactionService _dbTransactionService;

        public SignUpCommandHandler(
            UserManager<ApplicationUser> userManager,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtService jwtService,
            ITenantRepository tenantRepository,
            IDbTransactionService dbTransactionService)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
            _tenantRepository = tenantRepository;
            _dbTransactionService = dbTransactionService;
        }

        public async Task<SignupResult> Handle(SignupCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already registered.");

            var slugTaken = await _tenantRepository.IsSlugTakenAsync(request.TenantSlug, cancellationToken);
            if (slugTaken)
                throw new InvalidOperationException("Workspace slug already taken.");

            await using var transaction = await _dbTransactionService.BeginTransactionAsync(cancellationToken);

            try
            {
                var tenant = new Domain.Entities.Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = request.TenantName,
                    Slug = request.TenantSlug,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _tenantRepository.CreateAsync(tenant, cancellationToken);

                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    FullName = request.FullName,
                    Email = request.Email,
                    UserName = request.Email,
                    TenantId = tenant.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

                await _userManager.AddToRoleAsync(user, "Admin");

                var accessToken = _jwtService.GenerateToken(user, "Admin");
                var refreshToken = _jwtService.GenerateRefreshToken(user.Id);
                await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return new SignupResult(accessToken, refreshToken.Token);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

        }
    }
}
