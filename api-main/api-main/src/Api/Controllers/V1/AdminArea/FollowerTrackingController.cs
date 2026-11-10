using Application.FollowerWork.Common;
using Application.FollowerWork.Queries.Admin.GetLiveLocations;
using Application.FollowerWork.Queries.Admin.GetRoute;

namespace Api.Controllers.V1.AdminArea;

/// <summary>
/// قراءة تتبع المتابعين — Location.Read و Route.Read مستقلتان.
/// لا تُرجع مواقع موظفي المبيعات أو مندوبي الجباية.
/// </summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class FollowerTrackingController : ApiController
{
    private readonly IMediator _mediator;

    public FollowerTrackingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.FollowerWork.LocationRead)]
    [HttpGet("latest")]
    public async Task<ActionResult<FollowerLiveLocationsResponse>> GetLatest(
        [FromQuery] int? branchId = null,
        [FromQuery] int? followerId = null)
    {
        var result = await _mediator.Send(
            new GetFollowerLiveLocationsQuery
            {
                BranchId = branchId,
                FollowerId = followerId
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.FollowerWork.RouteRead)]
    [HttpGet("{followerId:int}/route")]
    public async Task<ActionResult<FollowerRouteResponse>> GetRoute(
        [FromRoute] int followerId,
        [FromQuery] int? branchId = null,
        [FromQuery] DateOnly? date = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        var result = await _mediator.Send(
            new GetFollowerRouteQuery
            {
                FollowerId = followerId,
                BranchId = branchId,
                Date = date,
                Page = page,
                PageSize = pageSize
            },
            CancellationToken);
        return result.ToHttpResponse();
    }
}
