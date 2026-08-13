using Application.Common.Models.KeysetPagination;
using Application.ProductCategories.Queries.App.GetPaginated;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class ProductCategoriesController : ApiController
{
    private readonly IMediator _mediator;

    public ProductCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.ProductCategory.Read)]
    [HttpGet]
    public async Task<ActionResult<List<GetPaginatedProductCategoriesResponse>>> GetPaginated(
        [FromQuery] KeysetPagination<DateTimeOffset?> keysetPagination,
        [FromQuery] GetPaginatedProductCategoriesFilter filter)
    {
        var query = new GetPaginatedProductCategoriesQuery
        {
            Pagination = keysetPagination,
            Filter = filter
        };
        var result = await _mediator.Send(query, CancellationToken);

        return Ok(result);
    }
}
