using Application.DelegateHome.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/DelegateHome")]
public class DelegateHomeController : ApiController
{
    private readonly IMediator _mediator;

    public DelegateHomeController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.DelegateHome.Read)]
    [HttpGet("summary")]
    public async Task<ActionResult<DelegateHomeSummaryDto>> Summary([FromQuery] int? branchId = null)
        => (await _mediator.Send(new GetDelegateHomeSummaryQuery { BranchId = branchId }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.DelegateHome.Read)]
    [HttpGet("account")]
    public async Task<ActionResult<DelegateAccountDto>> Account()
        => (await _mediator.Send(new GetDelegateAccountQuery(), CancellationToken)).ToHttpResponse();
}
