using Application.Safes.Commands.Admin.CreateCashDeliveries;
using Application.Safes.Commands.Admin.CreateCashDelivery;
using Application.Safes.Commands.Admin.CreateSafeTransfer;
using Application.Safes.Queries.Admin.GetAll;
using Application.Safes.Queries.Admin.GetByBranch;
using Application.Safes.Queries.Admin.GetSellers;
using Application.Safes.Queries.Admin.GetSellersExcelReport;
using Application.Safes.Queries.Admin.GetTransactions;
using Application.Safes.Queries.Admin.GetTransactionsExcelReport;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class SafesController : ApiController
{
    private readonly IMediator _mediator;

    public SafesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Safe.Read)]
    [HttpGet("current")]
    public async Task<ActionResult<GetSafeByBranchResponse>> GetByBranch([FromQuery] GetSafeByBranchQuery query)
    {
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Safe.Read)]
    [HttpGet("{id:int}/sellers")]
    public async Task<ActionResult<PaginatedList<GetSafeSellersResponse>>> GetSafeSellers([FromRoute] int id,
        [FromQuery] Pagination pagination, [FromQuery] GetSafeSellersFilter filter)
    {
        var query = new GetSafeSellersQuery
        {
            SafeId = id,
            Pagination = pagination,
            Filter = filter
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Transaction.CollectCash)]
    [HttpPost("{id:int}/sellers/{sellerId:int}/cash-deliveries")]
    public async Task<ActionResult> CreateCashDelivery([FromRoute] int id, [FromRoute] int sellerId,
        [FromBody] CreateCashDeliveryTransactionDto dto)
    {
        var command = new CreateCashDeliveryCommand
        {
            SafeId = id,
            SellerId = sellerId,
            Amount = dto.Amount,
            Date = dto.Date,
            Description = dto.Description
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Transaction.CollectCash)]
    [HttpPost("{id:int}/cash-deliveries")]
    public async Task<ActionResult> CreateCashDeliveries([FromRoute] int id, [FromBody] CreateCashDeliveriesDto dto)
    {
        var command = new CreateCashDeliveriesCommand
        {
            SafeId = id,
            SellerIds = dto.SellerIds
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Transaction.Read)]
    [HttpGet("{id:int}/transactions")]
    public async Task<ActionResult<PaginatedList<GetSafeTransactionsResponse>>> GetSafeTransactions([FromRoute] int id,
        [FromQuery] Pagination pagination, [FromQuery] GetSafeTransactionsFilter filter)
    {
        var query = new GetSafeTransactionsQuery
        {
            SafeId = id,
            Pagination = pagination,
            Filter = filter
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Transaction.Create)]
    [HttpPost("safe-transfer")]
    public async Task<ActionResult> CreateSafeTransfer([FromBody] CreateSafeTransferCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
    
    [HasPermission(Permissions.Safe.Read)]
    [HttpGet]
    public async Task<ActionResult<List<GetAllSafesResponse>>> GetAll()
    {
        var query = new GetAllSafesQuery();
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
    
    [HasPermission(Permissions.Safe.Read)]
    [HttpGet("{id:int}/sellers/excel-report")]
    public async Task<ActionResult> GetSafeSellersExcelReport([FromRoute] int id,
        [FromQuery] GetSafeSellersFilter filter)
    {
        var query = new GetSafeSellersExcelReportQuery
        {
            SafeId = id,
            Filter = filter
        };
        var result = await _mediator.Send(query, CancellationToken);

        var excelFile = result.Data;

        return File(excelFile!.Data, excelFile.ContentType, excelFile.FileName);
    }
    
    [HasPermission(Permissions.Transaction.Read)]
    [HttpGet("{id:int}/transactions/excel-report")]
    public async Task<ActionResult> GetSafeTransactionsExcelReport([FromRoute] int id)
    {
        var query = new GetSafeTransactionsExcelReportQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        var excelFile = result.Data;

        return File(excelFile!.Data, excelFile.ContentType, excelFile.FileName);
    }
}