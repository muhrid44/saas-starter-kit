using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaasStarterKit.Application.Common.Jobs;
using SaasStarterKit.Application.Users.Commands.CreateUser;
using SaasStarterKit.Application.Users.Commands.Login;
using SaasStarterKit.Application.Users.Commands.RefreshToken;

namespace SaasStarterKit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
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
            try
            {
                var token = await _mediator.Send(command, cancellationToken);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(command, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
    }
}
