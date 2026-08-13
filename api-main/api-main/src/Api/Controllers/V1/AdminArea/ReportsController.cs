using Application.Reports.Query.GetProductsReport;
using Application.Reports.Query.GetSafeReport;
using Application.Reports.Query.GetUsersReport;
using Application.Reports.Query.GetYearlyFinancialReport;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ReportsController : ApiController
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Report.Read)]
    [HttpGet("yearly-financial")]
    public async Task<ActionResult<GetYearlyFinancialReportResponse>> GetYearlyFinancialReport([FromQuery] int branchId,
        [FromQuery] int? year)
    {
        var query = new GetYearlyFinancialReportQuery
        {
            BranchId = branchId,
            Year = year
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Report.Read)]
    [HttpGet("users")]
    public async Task<ActionResult<GetUsersReportResponse>> GetUsersReport([FromQuery] GetUsersReportQuery query)
    {
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Report.Read)]
    [HttpGet("safe")]
    public async Task<ActionResult<GetSafeReportResponse>> GetSafeReport([FromQuery] GetSafeReportQuery query)
    {
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Report.Read)]
    [HttpGet("products")]
    public async Task<ActionResult<GetProductsReportResponse>> GetProductsReport([FromQuery] GetProductsReportQuery query)
    {
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}