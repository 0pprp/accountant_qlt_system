using Application.SalesPriceBoard.Queries.App.GetPriceBoard;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class SalesPriceBoardController : ApiController
{
    private readonly IMediator _mediator;

    public SalesPriceBoardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Product.Read)]
    [HttpGet("price-board")]
    public async Task<ActionResult<List<GetSalesPriceBoardResponse>>> PriceBoard()
    {
        var result = await _mediator.Send(new GetSalesPriceBoardQuery(), CancellationToken);
        return result.ToHttpResponse();
    }
}
