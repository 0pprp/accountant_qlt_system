using Application.Provinces.Queries.Admin.GetPaginated;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ProvincesController : ApiController
{
    private readonly IMediator _mediator;

    public ProvincesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Province.Read)]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<GetPaginatedProvincesResponse>>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedProvincesQuery 
        { 
            Pagination = pagination,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}