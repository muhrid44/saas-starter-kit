using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SaasStarterKit.Application.AuditLogs.Queries;

namespace SaasStarterKit.API.Controllers
{
    [EnableRateLimiting("general")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditLogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs()
        {
            var result = await _mediator.Send(new GetAuditLogsQuery());
            return Ok(result);
        }
    }
}
