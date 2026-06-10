using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SaasStarterKit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _mediator.Send(new UsersDto());
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserModel user, CancellationToken cancellationToken)
        {
            var newUserId = await _mediator.Send(user, cancellationToken);
            return Ok(newUserId);
        }
    }
}
