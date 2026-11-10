using Application.BranchDeclining.Common;
using Application.BranchDeclining.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/BranchDeclining")]
public class BranchDecliningController : ApiController
{
    private readonly IMediator _mediator;

    public BranchDecliningController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.BranchDeclining.Read)]
    [HttpGet]
    public async Task<ActionResult<BranchDecliningResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => (await _mediator.Send(new ListBranchDecliningQuery
        {
            Page = page,
            PageSize = pageSize
        }, CancellationToken)).ToHttpResponse();
}
