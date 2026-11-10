using Application.ManagementSummary.Common;
using Application.ManagementSummary.Queries.Admin.GetDetails;
using Application.ManagementSummary.Queries.Admin.GetMatrix;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ManagementSummaryController : ApiController
{
    private readonly IMediator _mediator;

    public ManagementSummaryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.ManagementSummary.Read)]
    [HttpGet("matrix")]
    public async Task<ActionResult<ManagementSummaryMatrixResponse>> GetMatrix(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string metric = "receipts",
        [FromQuery] int? branchId = null)
    {
        var result = await _mediator.Send(
            new GetManagementSummaryMatrixQuery
            {
                Year = year,
                Month = month,
                Metric = metric,
                BranchId = branchId
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementSummary.Read)]
    [HasPermission(Permissions.ManagementSummary.Drilldown)]
    [HttpGet("details")]
    public async Task<ActionResult<ManagementSummaryDetailsResponse>> GetDetails(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int day,
        [FromQuery] string metric,
        [FromQuery] int branchId,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        var result = await _mediator.Send(
            new GetManagementSummaryDetailsQuery
            {
                Year = year,
                Month = month,
                Day = day,
                Metric = metric,
                BranchId = branchId,
                Page = page,
                PageSize = pageSize
            },
            CancellationToken);
        return result.ToHttpResponse();
    }
}
