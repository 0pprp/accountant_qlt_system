using Application.PriceBoard.Common;
using Application.PriceBoard.Queries;

namespace Api.Controllers.V1.AppArea;

/// <summary>
/// إعلانات أسعار المحافظة لموظف المبيعات — منفصل عن SalesPriceBoard (منتجات المخزن).
/// </summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/PriceBoardAds")]
public class PriceBoardAdsController : ApiController
{
    private readonly IMediator _mediator;

    public PriceBoardAdsController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.PriceBoard.Read)]
    [HttpGet]
    public async Task<ActionResult<PriceBoardListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
        => (await _mediator.Send(new ListPriceBoardQuery
        {
            Page = page,
            PageSize = pageSize,
            ScopeToUserBranches = true
        }, CancellationToken)).ToHttpResponse();
}
