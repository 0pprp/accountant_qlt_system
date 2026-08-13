using Application.OrderLists.Queries.App.GetAll;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class OrderListsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public OrderListsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet]
    public async Task<ActionResult<List<GetAllOrderListsResponse>>> GetAll()
    {
        var query = new GetAllOrderListsQuery(_currentUserService.UserId!.Value);
        var result = await _mediator.Send(query, CancellationToken);

        return result.ToHttpResponse();
    }
}