using Application.Permissions.Queries.Admin.GetAll;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class PermissionsController : ApiController
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HasPermission(Permissions.Permission.Read)]
    [HttpGet]
    public async Task<ActionResult<GetAllPermissionsResponse>> GetAll()
    {
        var query = new GetAllPermissionsQuery();
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}