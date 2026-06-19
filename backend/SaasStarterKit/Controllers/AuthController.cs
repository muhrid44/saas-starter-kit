using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SaasStarterKit.Application.Common.Jobs;
using SaasStarterKit.Application.Users.Commands.Login;
using SaasStarterKit.Application.Users.Commands.RefreshToken;
using SaasStarterKit.Application.Users.Commands.Tenant;
using SaasStarterKit.Application.Users.Commands.User;

namespace SaasStarterKit.API.Controllers
{
    [EnableRateLimiting("auth")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var userId = await _mediator.Send(command, cancellationToken);

            // fire and forget welcome email
            BackgroundJob.Enqueue<EmailJob>(job => job.SendWelcomeEmail(command.Email));

            return Ok(new { UserId = userId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
        {
            var token = await _mediator.Send(command, cancellationToken);
            return Ok(new { Token = token });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Signup([FromBody] SignupCommand command)
        {
            var token = await _mediator.Send(command);
            return Ok(new { Token = token });
        }
    }
}
