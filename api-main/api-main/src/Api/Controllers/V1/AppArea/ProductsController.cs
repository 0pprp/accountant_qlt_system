using Application.Common.Models.KeysetPagination;
using Application.Products.Queries.App.GetPaginated;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class ProductsController : ApiController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Product.Read)]
    [HttpGet]
    public async Task<ActionResult<List<GetPaginatedProductsResponse>>> GetPaginated(
        [FromQuery] KeysetPagination<DateTimeOffset?> keysetPagination,
        [FromQuery] GetPaginatedProductsFilter filter)
    {
        var query = new GetPaginatedProductsQuery
        {
            Pagination = keysetPagination,
            Filter = filter
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}
