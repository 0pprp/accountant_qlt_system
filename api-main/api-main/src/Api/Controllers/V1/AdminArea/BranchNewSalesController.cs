using Application.BranchNewSales.Commands;
using Application.BranchNewSales.Common;
using Application.BranchNewSales.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/BranchNewSales")]
public class BranchNewSalesController : ApiController
{
    private readonly IMediator _mediator;

    public BranchNewSalesController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.BranchNewSale.Read)]
    [HttpGet]
    public async Task<ActionResult<BranchNewSaleListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool unreadOnly = false)
        => (await _mediator.Send(new ListBranchNewSalesQuery
        {
            Page = page,
            PageSize = pageSize,
            UnreadOnly = unreadOnly
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.BranchNewSale.Read)]
    [HttpGet("unread-count")]
    public async Task<ActionResult<BranchNewSaleUnreadCountDto>> UnreadCount()
        => (await _mediator.Send(new GetBranchNewSalesUnreadCountQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.BranchNewSale.Read)]
    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<BranchNewSaleItemDto>> Get([FromRoute] int orderId)
        => (await _mediator.Send(new GetBranchNewSaleQuery { OrderId = orderId }, CancellationToken))
            .ToHttpResponse();

    [HasPermission(Permissions.BranchNewSale.MarkRead)]
    [HttpPost("{orderId:int}/mark-read")]
    public async Task<ActionResult> MarkRead([FromRoute] int orderId)
        => (await _mediator.Send(new MarkBranchNewSaleReadCommand { OrderId = orderId }, CancellationToken))
            .ToHttpResponse();

    [HasPermission(Permissions.BranchNewSale.MarkRead)]
    [HttpPost("mark-all-read")]
    public async Task<ActionResult<int>> MarkAllRead()
        => (await _mediator.Send(new MarkAllBranchNewSalesReadCommand(), CancellationToken)).ToHttpResponse();
}
