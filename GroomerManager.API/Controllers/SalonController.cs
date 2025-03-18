using GroomerManager.API.Controllers.Common;
using GroomerManager.Application.Salon;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GroomerManager.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SalonController : BaseController
{
    private readonly IMediator _mediator;

    public SalonController(ILogger<SalonController> logger, IMediator mediator) : base(logger, mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<GetSalonsCommand.GetSalonsResponse>>> GetSalons()
    {
        var result = await _mediator.Send(new GetSalonsCommand.GetSalonsRequest());
        return Ok(result);
    }
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CreateSalonCommand.CreateSalonResponse>> CreateSalon([FromForm] CreateSalonCommand.CreateSalonRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}