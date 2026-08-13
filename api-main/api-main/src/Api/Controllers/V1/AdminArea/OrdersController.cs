using Application.Orders.Commands.Admin.ChangeApprovalStatus;
using Application.Orders.Commands.Admin.Create;
using Application.Orders.Commands.Admin.SetSellerInfo;
using Application.Orders.Commands.Admin.Update;
using Application.Orders.Queries.Admin.GetAvailableColumns;
using Application.Orders.Queries.Admin.GetById;
using Application.Orders.Queries.Admin.GetExcelReport;
using Application.Orders.Queries.Admin.GetInstallmentPayments;
using Application.Orders.Queries.Admin.GetPaginated;
using Application.Orders.Queries.Admin.GetSummary;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class OrdersController : ApiController
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Order.Create)]
    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> Create([FromForm] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateOrderDto dto)
    {
        var command = new UpdateOrderCommand
        {
            Id = id,
            OrderItems = dto.OrderItems,
            CreationAddress = dto.CreationAddress,
            SaleDate = dto.SaleDate,
            SaleTime = dto.SaleTime,
            SellerId = dto.SellerId,
            OrderListId = dto.OrderListId
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPut("{id:int}/seller-info")]
    public async Task<ActionResult> SetSellerInfo([FromRoute] int id, [FromBody] SetOrderSellerInfoDto dto)
    {
        var command = new SetOrderSellerInfoCommand
        {
            OrderId = id,
            CreationAddress = dto.CreationAddress,
            OrderListId = dto.OrderListId,
            SaleDate = dto.SaleDate,
            SaleTime = dto.SaleTime,
            SellerId = dto.SellerId
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet]
    public async Task<ActionResult<GetPaginatedOrdersResponse>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] GetOrdersPaginatedFilter filter, [FromQuery] List<string>? columns, [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedOrdersQuery
        {
            Pagination = pagination,
            Filter = filter,
            SelectedColumns = columns,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("available-columns")]
    public async Task<ActionResult<List<GetAvailableOrderColumnsResponse>>> GetAvailableColumns()
    {
        var query = new GetAvailableOrderColumnsQuery();
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("excel-report")]
    public async Task<ActionResult<GetOrdersExcelReportResponse>> GetExcelReport([FromQuery] GetOrdersPaginatedFilter filter,
        [FromQuery] List<string> columns)
    {
        var query = new GetOrdersExcelReportQuery
        {
            Filter = filter,
            SelectedColumns = columns
        };
        var result = await _mediator.Send(query, CancellationToken);

        var excelFile = result.Data;

        return File(excelFile!.Data, excelFile.ContentType, excelFile.FileName);
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetOrderByIdResponse>> GetById([FromRoute] int id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.InstallmentPayment.Read)]
    [HttpGet("{id:int}/installment-payments")]
    public async Task<ActionResult<List<GetOrderInstallmentPaymentsResponse>>> GetInstallmentPayments([FromRoute] int id)
    {
        var query = new GetOrderInstallmentPaymentsQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("{id:int}/summary")]
    public async Task<ActionResult<GetOrderSummaryResponse>> GetSummary([FromRoute] int id)
    {
        var query = new GetOrderSummaryQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPatch("{id:int}/approval-status")]
    public async Task<ActionResult> ChangeApprovalStatus([FromRoute] int id, [FromBody] ChangeOrderApprovalStatusDto dto)
    {
        var command = new ChangeOrderApprovalStatusCommand
        {
            OrderId = id,
            ApprovalStatus = dto.ApprovalStatus
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
}