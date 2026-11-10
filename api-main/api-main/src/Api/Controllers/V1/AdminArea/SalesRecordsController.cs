using Application.SalesRecords.Common;
using Application.SalesRecords.Queries.Admin.List;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class SalesRecordsController : ApiController
{
    private readonly IMediator _mediator;

    public SalesRecordsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>المبيعات المنجزة = Orders معتمدة. يستخدم Order.Read — لا يوسّع صلاحيات الطلبات.</summary>
    [HasPermission(Permissions.Order.Read)]
    [HttpGet]
    public async Task<ActionResult<ListSalesRecordsResponse>> List(
        [FromQuery] int? branchId,
        [FromQuery] int? sellerId,
        [FromQuery] string? search,
        [FromQuery] DateOnly? saleDateFrom,
        [FromQuery] DateOnly? saleDateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sort = null)
    {
        var result = await _mediator.Send(
            new ListSalesRecordsQuery
            {
                BranchId = branchId,
                SellerId = sellerId,
                Search = search,
                SaleDateFrom = saleDateFrom,
                SaleDateTo = saleDateTo,
                Page = page,
                PageSize = pageSize,
                Sort = sort,
            },
            CancellationToken);
        return result.ToHttpResponse();
    }
}
