using Application.OrderLists.Commands.Admin.Create;
using Application.OrderLists.Commands.Admin.Update;
using Application.OrderLists.Queries.Admin.GetExcelReport;
using Application.OrderLists.Queries.Admin.GetOrderListPaymentInstallments;
using Application.OrderLists.Queries.Admin.GetPaginated;
using Application.OrderLists.Queries.Admin.GetPaginatedReport;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class OrderListsController : ApiController
{
    private readonly IMediator _mediator;

    public OrderListsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.OrderList.Create)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateOrderListCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateOrderListDto dto)
    {
        var command = new UpdateOrderListCommand
        {
            Id = id,
            Name = dto.Name,
            MandobId = dto.MandobId,
            MotabaId = dto.MotabaId,
            BranchId = dto.BranchId
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<GetPaginatedOrderListsResponse>>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedOrderListsFilter filter, [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedOrderListsQuery
        {
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("excel-report")]
    public async Task<ActionResult<GetOrderListsExcelReportResponse>> GetExcelReport([FromQuery] GetPaginatedOrderListsFilter filter)
    {
        var query = new GetOrderListsExcelReportQuery(filter);
        var result = await _mediator.Send(query, CancellationToken);

        var excelFile = result.Data;

        return File(excelFile!.Data, excelFile.ContentType, excelFile.FileName);
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet("reports")]
    public async Task<ActionResult<GetPaginatedOrderListReportsResponse>> GetReports([FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedOrderListReportsFilter filter, [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedOrderListReportsQuery
        {
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.InstallmentPayment.Read)]
    [HttpGet("{id:int}/installment-payments")]
    public async Task<ActionResult<PaginatedList<GetOrderListInstallmentPaymentsResponse>>> GetOrderListInstallmentPayments(
        [FromRoute] int id, [FromQuery] Pagination pagination, [FromQuery] GetOrderListInstallmentPaymentsFilter filter,
        [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetOrderListInstallmentPaymentsQuery
        {
            OrderListId = id,
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}