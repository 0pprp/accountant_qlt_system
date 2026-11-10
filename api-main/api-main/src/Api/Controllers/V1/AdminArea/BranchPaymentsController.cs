using Application.BranchPayments.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/BranchPayments")]
public class BranchPaymentsController : ApiController
{
    private readonly IMediator _mediator;

    public BranchPaymentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// تسديدات قائمة ضمن فترة — قراءة فقط لنطاق فرع الجلسة (مصدر BM payments_page).
    /// </summary>
    [HasPermission(Permissions.OrderList.Read)]
    [HttpGet]
    public async Task<ActionResult<BranchPaymentsListResponse>> List(
        [FromQuery] int listId,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
        => (await _mediator.Send(new ListBranchPaymentsQuery
        {
            ListId = listId,
            FromDate = fromDate,
            ToDate = toDate
        }, CancellationToken)).ToHttpResponse();
}
