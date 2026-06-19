using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SaasStarterKit.Application.Users.Commands.User;
using SaasStarterKit.Application.Users.Queries.GetUsers;

namespace SaasStarterKit.API.Controllers
{
    [EnableRateLimiting("general")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("role")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPut("password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
