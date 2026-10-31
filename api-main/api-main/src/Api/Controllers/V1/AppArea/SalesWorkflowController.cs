using Application.SalesWorkflow.Commands.App.Complete;
using Application.SalesWorkflow.Commands.App.Progress;
using Application.SalesWorkflow.Commands.App.Transfer;
using Application.SalesWorkflow.Queries.App.GetBins;
using Application.SalesWorkflow.Queries.App.GetByStatus;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class SalesWorkflowController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public SalesWorkflowController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("bins")]
    public async Task<ActionResult<GetSalesWorkflowBinsResponse>> Bins()
    {
        var result = await _mediator.Send(new GetSalesWorkflowBinsQuery(_currentUserService.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("by-status")]
    public async Task<ActionResult<List<SalesWorkflowItemDto>>> ByStatus([FromQuery] string status)
    {
        var query = new GetSalesWorkflowByStatusQuery
        {
            UserId = _currentUserService.UserId!.Value,
            Status = status
        };
        var result = await _mediator.Send(query, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPost("{id:int}/progress")]
    public async Task<ActionResult<SalesWorkflowItemDto>> Progress([FromRoute] int id)
    {
        var command = new ProgressSalesWorkflowCommand
        {
            OrderId = id,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(command, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<SalesWorkflowItemDto>> Complete([FromRoute] int id)
    {
        var command = new CompleteSalesWorkflowCommand
        {
            OrderId = id,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(command, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPost("{id:int}/transfer")]
    public async Task<ActionResult<SalesWorkflowItemDto>> Transfer([FromRoute] int id, [FromBody] TransferSalesWorkflowDto dto)
    {
        var command = new TransferSalesWorkflowCommand
        {
            OrderId = id,
            UserId = _currentUserService.UserId!.Value,
            TargetStatus = dto.TargetStatus,
            Reason = dto.Reason
        };
        var result = await _mediator.Send(command, CancellationToken);
        return result.ToHttpResponse();
    }
}
