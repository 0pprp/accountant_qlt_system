using Application.PriceBoard.Commands;
using Application.PriceBoard.Common;
using Application.PriceBoard.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/PriceBoard")]
public class PriceBoardController : ApiController
{
    private readonly IMediator _mediator;

    public PriceBoardController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.PriceBoard.Read)]
    [HttpGet]
    public async Task<ActionResult<PriceBoardListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? provinceId = null,
        [FromQuery] string? search = null,
        [FromQuery] bool scopeToUserBranches = false)
        => (await _mediator.Send(new ListPriceBoardQuery
        {
            Page = page,
            PageSize = pageSize,
            ProvinceId = provinceId,
            Search = search,
            ScopeToUserBranches = scopeToUserBranches
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.PriceBoard.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PriceBoardEntryDto>> Get([FromRoute] int id)
        => (await _mediator.Send(new GetPriceBoardByIdQuery { Id = id }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.PriceBoard.Create)]
    [HttpPost]
    public async Task<ActionResult<PriceBoardEntryDto>> Create([FromBody] CreateBody body)
        => (await _mediator.Send(new CreatePriceBoardEntryCommand
        {
            ProductName = body.ProductName ?? "",
            SalePrice = body.SalePrice,
            DailyInstallment = body.DailyInstallment,
            DownPayment = body.DownPayment,
            TargetProvinceId = body.TargetProvinceId
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.PriceBoard.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PriceBoardEntryDto>> Update([FromRoute] int id, [FromBody] UpdateBody body)
        => (await _mediator.Send(new UpdatePriceBoardEntryCommand
        {
            Id = id,
            ProductName = body.ProductName ?? "",
            SalePrice = body.SalePrice,
            DailyInstallment = body.DailyInstallment,
            DownPayment = body.DownPayment,
            TargetProvinceId = body.TargetProvinceId,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.PriceBoard.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete([FromRoute] int id, [FromQuery] int expectedVersion)
        => (await _mediator.Send(new DeletePriceBoardEntryCommand
        {
            Id = id,
            ExpectedVersion = expectedVersion
        }, CancellationToken)).ToHttpResponse();

    public sealed class CreateBody
    {
        public string? ProductName { get; set; }
        public decimal SalePrice { get; set; }
        public decimal DailyInstallment { get; set; }
        public decimal DownPayment { get; set; }
        public int TargetProvinceId { get; set; }
    }

    public sealed class UpdateBody
    {
        public string? ProductName { get; set; }
        public decimal SalePrice { get; set; }
        public decimal DailyInstallment { get; set; }
        public decimal DownPayment { get; set; }
        public int TargetProvinceId { get; set; }
        public int ExpectedVersion { get; set; }
    }
}
