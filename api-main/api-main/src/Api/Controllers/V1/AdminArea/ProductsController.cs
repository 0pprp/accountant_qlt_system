using Application.Products.Commands.Admin.Create;
using Application.Products.Commands.Admin.Delete;
using Application.Products.Commands.Admin.Update;
using Application.Products.Queries.Admin.GetExcelReport;
using Application.Products.Queries.Admin.GetPaginated;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ProductsController : ApiController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Product.Create)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Product.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateProductDto dto)
    {
        var command = new UpdateProductCommand
        {
            Id = id,
            Name = dto.Name,
            RemainingCount = dto.RemainingCount,
            BuyAmount = dto.BuyAmount,
            SellAmount = dto.SellAmount,
            DailyInstallmentAmount = dto.DailyInstallmentAmount,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            WarehouseId = dto.WarehouseId
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Product.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Product.Read)]
    [HttpGet("excel-report")]
    public async Task<ActionResult> GetExcelReport([FromQuery] GetProductsExcelReportQuery query)
    {
        var result = await _mediator.Send(query, CancellationToken);

        var excelFile = result.Data;

        return File(excelFile!.Data, excelFile.ContentType, excelFile.FileName);
    }

    [HasPermission(Permissions.Product.Read)]
    [HttpGet]
    public async Task<ActionResult<GetPaginatedProductsResponse>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedProductsFilter filter, [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedProductsQuery
        {
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}