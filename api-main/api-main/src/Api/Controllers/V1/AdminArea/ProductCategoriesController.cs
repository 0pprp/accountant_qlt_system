using Application.ProductCategories.Commands.Admin.Create;
using Application.ProductCategories.Commands.Admin.Delete;
using Application.ProductCategories.Commands.Admin.Update;
using Application.ProductCategories.Queries.Admin.GetPaginated;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ProductCategoriesController : ApiController
{
    private readonly IMediator _mediator;

    public ProductCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.ProductCategory.Create)]
    [HttpPost]
    public async Task<ActionResult<CreateProductCategoryResponse>> Create([FromBody] CreateProductCategoryCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ProductCategory.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateProductCategoryDto dto)
    {
        var command = new UpdateProductCategoryCommand
        {
            Id = id,
            Name = dto.Name
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ProductCategory.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        var command = new DeleteProductCategoryCommand(id);
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ProductCategory.Read)]
    [HttpGet]
    public async Task<ActionResult<GetPaginatedProductCategoriesResponse>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedProductCategoriesFilter filter, [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedProductCategoriesQuery
        {
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}