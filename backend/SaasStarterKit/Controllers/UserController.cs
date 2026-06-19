using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SaasStarterKit.Application.Users.Commands.User;
using SaasStarterKit.Application.Users.Queries.Dashboard;
using SaasStarterKit.Application.Users.Queries.GetUsers;

namespace SaasStarterKit.API.Controllers
{
    [EnableRateLimiting("general")]
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : Controller
    {
        private readonly IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("dashboard/info")]
        public async Task<IActionResult> GetDashboardInfo()
        {
            var info = await _mediator.Send(new DashboardInfo());
            return Ok(info);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _mediator.Send(new UsersDto());
            return Ok(users);
        }


        [HttpGet]
        public async Task<IActionResult> GetUserProfile()
        {
            var result = await _mediator.Send(new UserProfileRequest());
            return Ok(result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
