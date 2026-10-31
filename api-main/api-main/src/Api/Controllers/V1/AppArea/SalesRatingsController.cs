using Application.SalesRatings.Queries.App.GetByLevel;
using Application.SalesRatings.Queries.App.GetCustomerProfileLite;
using Application.SalesRatings.Queries.App.GetSummary;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class SalesRatingsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public SalesRatingsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("summary")]
    public async Task<ActionResult<GetSalesRatingsSummaryResponse>> Summary()
    {
        var result = await _mediator.Send(new GetSalesRatingsSummaryQuery(_currentUserService.UserId!.Value), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("by-level")]
    public async Task<ActionResult<List<GetSalesRatingsByLevelResponse>>> ByLevel([FromQuery] string level)
    {
        var query = new GetSalesRatingsByLevelQuery
        {
            UserId = _currentUserService.UserId!.Value,
            Level = level
        };
        var result = await _mediator.Send(query, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Customer.Read)]
    [HttpGet("customer/{id:int}/profile-lite")]
    public async Task<ActionResult<GetSalesCustomerProfileLiteResponse>> CustomerProfileLite([FromRoute] int id)
    {
        var query = new GetSalesCustomerProfileLiteQuery
        {
            UserId = _currentUserService.UserId!.Value,
            CustomerId = id
        };
        var result = await _mediator.Send(query, CancellationToken);
        return result.ToHttpResponse();
    }
}
