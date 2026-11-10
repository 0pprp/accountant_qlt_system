using Application.SalesSummary.Common;
using Application.SalesSummary.Queries.Admin.GetDetails;
using Application.SalesSummary.Queries.Admin.GetMatrix;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class SalesSummaryController : ApiController
{
    private readonly IMediator _mediator;

    public SalesSummaryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.SalesSummary.Read)]
    [HttpGet("matrix")]
    public async Task<ActionResult<SalesSummaryMatrixResponse>> Matrix(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int? branchId,
        [FromQuery] string? metric)
    {
        var result = await _mediator.Send(
            new GetSalesSummaryMatrixQuery
            {
                Year = year,
                Month = month,
                BranchId = branchId,
                Metric = metric,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesSummary.Drilldown)]
    [HttpGet("details")]
    public async Task<ActionResult<SalesSummaryDetailsResponse>> Details(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int day,
        [FromQuery] int branchId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? metric = null)
    {
        var result = await _mediator.Send(
            new GetSalesSummaryDetailsQuery
            {
                Year = year,
                Month = month,
                Day = day,
                BranchId = branchId,
                Page = page,
                PageSize = pageSize,
                Metric = metric,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }
}
