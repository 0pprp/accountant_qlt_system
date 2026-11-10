using Application.SalesEmployeeTracking.Common;
using Application.SalesEmployeeTracking.Queries.Admin.GetLatest;
using Application.SalesEmployeeTracking.Queries.Admin.GetRoute;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/SalesEmployeeTracking")]
public class SalesEmployeeTrackingController : ApiController
{
    private readonly IMediator _mediator;

    public SalesEmployeeTrackingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.SalesLocation.Read)]
    [HttpGet("latest")]
    public async Task<ActionResult<SalesEmployeeLiveLocationsResponse>> Latest(
        [FromQuery] int? branchId,
        [FromQuery] int? employeeId)
    {
        var result = await _mediator.Send(
            new GetSalesEmployeeLiveLocationsQuery
            {
                BranchId = branchId,
                EmployeeId = employeeId,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>مسار يوم تقويمي بغداد — صلاحية SalesRoute.Read مستقلة عن الموقع المباشر.</summary>
    [HasPermission(Permissions.SalesRoute.Read)]
    [HttpGet("{employeeId:int}/route")]
    public async Task<ActionResult<SalesEmployeeRouteResponse>> Route(
        [FromRoute] int employeeId,
        [FromQuery] int? branchId,
        [FromQuery] DateOnly? date,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _mediator.Send(
            new GetSalesEmployeeRouteQuery
            {
                EmployeeId = employeeId,
                BranchId = branchId,
                Date = date,
                Page = page,
                PageSize = pageSize,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }
}
