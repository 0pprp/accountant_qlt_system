using Application.SalesOverview.Common;
using Application.SalesOverview.Queries.Admin.GetOverview;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class SalesOverviewController : ApiController
{
    private readonly IMediator _mediator;

    public SalesOverviewController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.SalesOverview.Read)]
    [HttpGet]
    public async Task<ActionResult<SalesOverviewResponse>> Get([FromQuery] int? branchId)
    {
        var result = await _mediator.Send(
            new GetSalesOverviewQuery { BranchId = branchId },
            CancellationToken);
        return result.ToHttpResponse();
    }
}
