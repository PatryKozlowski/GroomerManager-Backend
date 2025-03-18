using GroomerManager.API.Controllers.Common;
using GroomerManager.Application.User;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GroomerManager.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class UserController : BaseController
{
    private readonly IMediator _mediator;

    public UserController(ILogger<UserController> logger, IMediator mediator) : base(logger, mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<GetUserInfoCommand.GetUserInfoResponse>> GetUserInfo()
    {
        var result = await _mediator.Send(new GetUserInfoCommand.GetUserInfoRequest() {});
        return Ok(result);
    }
}