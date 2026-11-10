using Application.WrongSales.Commands;
using Application.WrongSales.Common;
using Application.WrongSales.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/WrongSales")]
public class WrongSalesController : ApiController
{
    private readonly IMediator _mediator;

    public WrongSalesController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.WrongSale.Read)]
    [HttpGet]
    public async Task<ActionResult<WrongSaleListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        [FromQuery] int? branchId = null,
        [FromQuery] string? mailbox = null)
        => (await _mediator.Send(new ListWrongSalesQuery
        {
            Page = page,
            PageSize = pageSize,
            BranchId = branchId,
            Mailbox = mailbox
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.WrongSale.MarkDelegateRead)]
    [HttpPost("{id:int}/mark-delegate-read")]
    public async Task<ActionResult<WrongSaleNoticeDto>> MarkDelegateRead(
        [FromRoute] int id,
        [FromBody] VersionBody body)
        => (await _mediator.Send(new MarkWrongSaleDelegateReadCommand
        {
            Id = id,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.WrongSale.MarkBranchReviewed)]
    [HttpPost("{id:int}/mark-branch-reviewed")]
    public async Task<ActionResult<WrongSaleNoticeDto>> MarkBranchReviewed(
        [FromRoute] int id,
        [FromBody] ReviewBody body)
        => (await _mediator.Send(new MarkWrongSaleBranchReviewedCommand
        {
            Id = id,
            Note = body.Note,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken)).ToHttpResponse();

    public sealed class VersionBody
    {
        public int ExpectedVersion { get; set; }
    }

    public sealed class ReviewBody
    {
        public int ExpectedVersion { get; set; }
        public string? Note { get; set; }
    }
}
