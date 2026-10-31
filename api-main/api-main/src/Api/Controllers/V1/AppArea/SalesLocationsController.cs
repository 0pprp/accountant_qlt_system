using Application.SalesLocations.Commands.App.IngestBatch;
using Application.SalesLocations.Commands.App.IngestLive;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class SalesLocationsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public SalesLocationsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPost("batch")]
    public async Task<ActionResult<IngestSalesLocationBatchResponse>> Batch([FromBody] List<SalesLocationPointDto> points)
    {
        var command = new IngestSalesLocationBatchCommand
        {
            UserId = _currentUserService.UserId!.Value,
            Points = points
        };
        var result = await _mediator.Send(command, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPost("live")]
    public async Task<ActionResult<IngestSalesLiveLocationResponse>> Live([FromBody] IngestSalesLiveLocationDto dto)
    {
        var command = new IngestSalesLiveLocationCommand
        {
            UserId = _currentUserService.UserId!.Value,
            Lat = dto.Lat,
            Lng = dto.Lng
        };
        var result = await _mediator.Send(command, CancellationToken);
        return result.ToHttpResponse();
    }
}
