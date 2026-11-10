using Application.ManagementRatings.Common;
using Application.ManagementRatings.Queries.Admin.GetBranches;
using Application.ManagementRatings.Queries.Admin.GetCatalog;
using Application.ManagementRatings.Queries.Admin.GetCustomers;
using Application.ManagementRatings.Queries.Admin.GetListPaymentsSum;
using Application.ManagementRatings.Queries.Admin.GetPeriodSummary;
using Application.ManagementRatings.Queries.Admin.GetTrend;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ManagementRatingsController : ApiController
{
    private readonly IMediator _mediator;

    public ManagementRatingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.ManagementRatings.Read)]
    [HttpGet("branches")]
    public async Task<ActionResult<ManagementRatingsBranchesResponse>> GetBranches()
    {
        var result = await _mediator.Send(new GetManagementRatingsBranchesQuery(), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementRatings.Read)]
    [HttpGet("catalog")]
    public async Task<ActionResult<ManagementRatingsCatalogResponse>> GetCatalog([FromQuery] int branchId)
    {
        var result = await _mediator.Send(
            new GetManagementRatingsCatalogQuery { BranchId = branchId },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementRatings.Read)]
    [HttpGet("period-summary")]
    public async Task<ActionResult<ManagementRatingsPeriodSummaryResponse>> GetPeriodSummary(
        [FromQuery] int branchId,
        [FromQuery] string scope = "branch",
        [FromQuery] int resourceId = 0)
    {
        var result = await _mediator.Send(
            new GetManagementRatingsPeriodSummaryQuery
            {
                BranchId = branchId,
                Scope = scope,
                ResourceId = resourceId
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementRatings.Read)]
    [HttpGet("trend")]
    public async Task<ActionResult<ManagementRatingsTrendResponse>> GetTrend(
        [FromQuery] int branchId,
        [FromQuery] string scope = "branch",
        [FromQuery] int resourceId = 0,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var result = await _mediator.Send(
            new GetManagementRatingsTrendQuery
            {
                BranchId = branchId,
                Scope = scope,
                ResourceId = resourceId,
                FromDate = fromDate,
                ToDate = toDate
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementRatings.DetailsRead)]
    [HttpGet("customers")]
    public async Task<ActionResult<ManagementRatingsCustomersResponse>> GetCustomers(
        [FromQuery] int branchId,
        [FromQuery] string scope = "branch",
        [FromQuery] int resourceId = 0,
        [FromQuery] string level = "",
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? q = null)
    {
        var result = await _mediator.Send(
            new GetManagementRatingsCustomersQuery
            {
                BranchId = branchId,
                Scope = scope,
                ResourceId = resourceId,
                Level = level,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize,
                Q = q
            },
            CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementRatings.Read)]
    [HttpGet("list-payments-sum")]
    public async Task<ActionResult<ManagementRatingsListPaymentsSumResponse>> GetListPaymentsSum(
        [FromQuery] int branchId,
        [FromQuery] int listId,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var result = await _mediator.Send(
            new GetManagementRatingsListPaymentsSumQuery
            {
                BranchId = branchId,
                ListId = listId,
                FromDate = fromDate,
                ToDate = toDate
            },
            CancellationToken);
        return result.ToHttpResponse();
    }
}
