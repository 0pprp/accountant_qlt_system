using Application.SalesShifts.Commands.App.End;
using Application.SalesShifts.Commands.App.Start;
using Application.SalesShifts.Queries.App.GetCurrent;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class SalesShiftsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public SalesShiftsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPost("start")]
    public async Task<ActionResult<StartSalesShiftResponse>> Start()
    {
        var result = await _mediator.Send(new StartSalesShiftCommand(_currentUserService.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPost("end")]
    public async Task<ActionResult<EndSalesShiftResponse>> End()
    {
        var result = await _mediator.Send(new EndSalesShiftCommand(_currentUserService.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("current")]
    public async Task<ActionResult<GetCurrentSalesShiftResponse>> Current()
    {
        var result = await _mediator.Send(new GetCurrentSalesShiftQuery(_currentUserService.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }
}
