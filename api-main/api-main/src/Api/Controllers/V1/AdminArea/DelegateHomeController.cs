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
    public async Task<ActionResult<DelegateHomeSummaryDto>> Summary()
        => (await _mediator.Send(new GetDelegateHomeSummaryQuery(), CancellationToken)).ToHttpResponse();
}
