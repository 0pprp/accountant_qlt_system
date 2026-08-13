using Application.Roles.Queries.Admin.GetAll;
using Application.Roles.Queries.Admin.GetRolePermissions;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class RolesController : ApiController
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Role.Read)]
    [HttpGet]
    public async Task<ActionResult<List<GetAllRolesResponse>>> GetAll()
    {
        var query = new GetAllRolesQuery();
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
    
    [HasPermission(Permissions.Permission.Read)]
    [HttpGet("{id:int}/permissions")]
    public async Task<ActionResult<GetRolePermissionsResponse>> GetRolePermissions([FromRoute] int id)
    {
        var query = new GetRolePermissionsQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}