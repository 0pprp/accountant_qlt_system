using Application.SalesExcelSearch.Common;
using Application.SalesExcelSearch.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/SalesExcelSearch")]
public class SalesExcelSearchController : ApiController
{
    private readonly IMediator _mediator;

    public SalesExcelSearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public sealed class SearchBody
    {
        public List<string>? Names { get; set; }
    }

    /// <summary>بحث أسماء زبائن من Excel — قراءة فقط ضمن نطاق فروع الجلسة.</summary>
    [HasPermission(Permissions.Customer.Read)]
    [HttpPost("customers")]
    public async Task<ActionResult<SalesExcelSearchResponse>> Search([FromBody] SearchBody body)
    {
        var result = await _mediator.Send(
            new SearchSalesCustomersByNamesQuery { Names = body?.Names ?? [] },
            CancellationToken);
        return result.ToHttpResponse();
    }
}
