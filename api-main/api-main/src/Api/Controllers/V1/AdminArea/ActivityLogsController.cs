using Application.ActivityLogs.Queries.Admin.GetById;
using Application.ActivityLogs.Queries.Admin.GetPaginated;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ActivityLogsController : ApiController
{
    private readonly IMediator _mediator;

    public ActivityLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.ActivityLog.Read)]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<GetPaginatedActivityLogsResponse>>> GetPaginated(
        [FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedActivityLogsFilter filter,
        [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedActivityLogsQuery
        {
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ActivityLog.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetActivityLogByIdResponse>> GetById([FromRoute] int id)
    {
        var query = new GetActivityLogByIdQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}