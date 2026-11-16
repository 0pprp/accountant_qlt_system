using Application.SalesInventory.Queries.App.GetInventory;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class SalesInventoryController : ApiController
{
    private readonly IMediator _mediator;

    public SalesInventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Product.Read)]
    [HttpGet("inventory")]
    public async Task<ActionResult<List<GetSalesInventoryResponse>>> Inventory([FromQuery] int? orderId = null, [FromQuery] int? requestId = null)
    {
        var result = await _mediator.Send(new GetSalesInventoryQuery(orderId, requestId), CancellationToken);
        return result.ToHttpResponse();
    }
}
