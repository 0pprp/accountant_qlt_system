using Application.Purchases.Commands.Admin.Create;
using Application.Purchases.Commands.Admin.Update;
using Application.Purchases.Queries.Admin.GetById;
using Application.Purchases.Queries.Admin.GetExcelReport;
using Application.Purchases.Queries.Admin.GetLastFactorNumber;
using Application.Purchases.Queries.Admin.GetPaginated;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class PurchasesController : ApiController
{
    private readonly IMediator _mediator;

    public PurchasesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HasPermission(Permissions.Purchase.Create)]
    [HttpPost]
    public async Task<ActionResult> Create([FromForm] CreatePurchaseCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
    
    [HasPermission(Permissions.Purchase.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdatePurchaseDto dto)
    {
        var command = new UpdatePurchaseCommand
        {
            Id = id,
            PurchaseItems = dto.PurchaseItems
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
    
    [HasPermission(Permissions.Purchase.Read)]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<GetPaginatedPurchasesResponse>>> GetPaginated([FromQuery] Pagination pagination,
        [FromQuery] GetPaginatedPurchasesFilter filter, [FromQuery] List<SortCriterion>? sortCriteria)
    {
        var query = new GetPaginatedPurchasesQuery
        {
            Pagination = pagination,
            Filter = filter,
            SortCriteria = sortCriteria
        };
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
    
    [HasPermission(Permissions.Purchase.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetPurchaseByIdResponse>> GetById([FromRoute] int id)
    {
        var query = new GetPurchaseByIdQuery(id);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
    
    [HasPermission(Permissions.Purchase.Read)]
    [HttpGet("excel-report")]
    public async Task<ActionResult> GetExcelReport([FromQuery] GetPurchasesExcelReportQuery query)
    {
        var result = await _mediator.Send(query, CancellationToken);

        var excelFile = result.Data;

        return File(excelFile!.Data, excelFile.ContentType, excelFile.FileName);
    }
    
    [HasPermission(Permissions.Purchase.Read)]
    [HttpGet("last-factor-number")]
    public async Task<ActionResult<GetLastPurchaseFactorNumberResponse>> GetLastFactorNumber()
    {
        var query = new GetLastPurchaseFactorNumberQuery();
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}