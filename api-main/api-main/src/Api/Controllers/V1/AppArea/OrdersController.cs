using Application.Common.Utilities;
using Application.InstallmentPayments.Commands.App.Create;
using Application.Orders.Commands.App.Create;
using Application.Orders.Commands.App.CompleteAttachments;
using Application.Orders.Commands.App.Delete;
using Application.Orders.Commands.App.SetSellerInfo;
using Application.Orders.Commands.App.Update;
using Application.Orders.Queries.App.GetAll;
using Application.Orders.Queries.App.GetById;
using Application.Orders.Queries.App.GetOrderInstallmentPayments;
using Application.Orders.Queries.App.GetOrderInstallmentPaymentsPdfReport;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class OrdersController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public OrdersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet]
    public async Task<ActionResult<List<GetAllOrdersResponse>>> GetAll([FromQuery] GetPaginatedOrdersFilter filter)
    {
        var query = new GetAllOrdersQuery
        {
            Filter = filter,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HasPermission(Permissions.InstallmentPayment.Read)]
    [HttpGet("{id:int}/installment-payments")]
    public async Task<ActionResult<GetOrderInstallmentPaymentsResponse>> GetOrderInstallmentPayments([FromRoute] int id)
    {
        var query = new GetOrderInstallmentPaymentsQuery
        {
            OrderId = id,
            UserId = _currentUserService.UserId!.Value,
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetOrderByIdResponse>> GetById([FromRoute] int id)
    {
        var query = new GetOrderByIdQuery
        {
            OrderId = id,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.InstallmentPayment.Read)]
    [HttpGet("{id:int}/installment-payments/pdf-report")]
    public async Task<ActionResult> GetInstallmentPaymentPdf([FromRoute] int id, [FromQuery] PdfSettings? pdfSettings)
    {
        var query = new GetOrderInstallmentPaymentsPdfReportQuery
        {
            OrderId = id,
            UserId = _currentUserService.UserId!.Value,
            PdfSettings = pdfSettings
        };
        var result = await _mediator.Send(query, CancellationToken);

        var pdfFile = result.Data;

        return File(pdfFile!.Data, pdfFile.ContentType, pdfFile.FileName);
    }

    [HasPermission(Permissions.InstallmentPayment.Create)]
    [HttpPost("{id:int}/installment-payments")]
    public async Task<ActionResult> CreateInstallmentPayment([FromRoute] int id, [FromBody] CreateInstallmentPaymentDto dto)
    {
        var command = new CreateInstallmentPaymentCommand
        {
            OrderId = id,
            Amount = dto.Amount,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Create)]
    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> Create([FromBody] CreateOrderDto dto)
    {
        var command = new CreateOrderCommand
        {
            OrderItems = dto.OrderItems,
            CustomerId = dto.CustomerId,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Update)]
    [HttpPost("{id:int}/complete-attachments")]
    public async Task<ActionResult> CompleteAttachments([FromRoute] int id)
    {
        var command = new CompleteOrderAttachmentsCommand
        {
            OrderId = id,
            UserId = _currentUserService.UserId!.Value
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
            Location = dto.Location,
            SaleDate = dto.SaleDate,
            SaleTime = dto.SaleTime,
            OrderListId = dto.OrderListId,
            UserId = _currentUserService.UserId!.Value
        };
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
            Location = dto.Location,
            SaleDate = dto.SaleDate,
            SaleTime = dto.SaleTime,
            OrderListId = dto.OrderListId,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Order.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        var command = new DeleteOrderCommand
        {
            OrderId = id,
            UserId = _currentUserService.UserId!.Value
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
}