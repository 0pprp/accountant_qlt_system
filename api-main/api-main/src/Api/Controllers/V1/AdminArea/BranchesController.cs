using Application.Branches.Commands.Admin.Create;
using Application.Branches.Commands.Admin.Delete;
using Application.Branches.Commands.Admin.Update;
using Application.Branches.Queries.Admin.GetBranchWarehouses;
using Application.Branches.Queries.Admin.GetPaginated;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class BranchesController : ApiController
{
    private readonly IMediator _mediator;

    public BranchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Branch.Create)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateBranchCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Branch.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateBranchDto dto)
    {
        var command = new UpdateBranchCommand()
        {
            Id = id,
            Name = dto.Name,
            ProvinceId = dto.ProvinceId
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Branch.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        var command = new DeleteBranchCommand(id);
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Branch.Read)]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<GetPaginatedBranchesResponse>>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedBranchesFilter filter, [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedBranchesQuery
        {
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Warehouse.Read)]
    [HttpGet("{branchId:int}/warehouses")]
    public async Task<ActionResult<List<GetBranchWarehousesResponse>>> GetBranchWarehouses([FromRoute] int branchId)
    {
        var query = new GetBranchWarehousesQuery(branchId);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}