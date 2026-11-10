using Application.BranchVisits.Commands;
using Application.BranchVisits.Common;
using Application.BranchVisits.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/BranchVisits")]
public class BranchVisitsController : ApiController
{
    private readonly IMediator _mediator;

    public BranchVisitsController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.BranchVisit.Read)]
    [HttpGet]
    public async Task<ActionResult<BranchVisitListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => (await _mediator.Send(new ListBranchVisitsQuery { Page = page, PageSize = pageSize }, CancellationToken))
            .ToHttpResponse();

    [HasPermission(Permissions.BranchVisit.Read)]
    [HttpGet("active")]
    public async Task<ActionResult<BranchVisitDto?>> Active()
        => (await _mediator.Send(new GetActiveBranchVisitQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.BranchVisit.Open)]
    [HttpPost("open")]
    public async Task<ActionResult<BranchVisitDto>> Open([FromBody] OpenBody body)
        => (await _mediator.Send(new OpenBranchVisitCommand { BranchId = body.BranchId }, CancellationToken))
            .ToHttpResponse();

    [HasPermission(Permissions.BranchVisit.Rate)]
    [HttpPut("{id:int}/ratings")]
    public async Task<ActionResult<BranchVisitDto>> Rate([FromRoute] int id, [FromBody] RateBody body)
        => (await _mediator.Send(new UpsertBranchVisitRatingCommand
        {
            VisitId = id,
            OrderListId = body.OrderListId,
            Score = body.Score,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.BranchVisit.Depart)]
    [HttpPost("{id:int}/depart")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<ActionResult<BranchVisitDto>> Depart(
        [FromRoute] int id,
        [FromForm] string note,
        [FromForm] int expectedVersion,
        [FromForm] List<IFormFile>? files)
        => (await _mediator.Send(new DepartBranchVisitCommand
        {
            VisitId = id,
            Note = note ?? "",
            ExpectedVersion = expectedVersion,
            Files = files
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.BranchVisit.Read)]
    [HttpGet("{visitId:int}/attachments/{attachmentId:int}")]
    public async Task<IActionResult> Download([FromRoute] int visitId, [FromRoute] int attachmentId)
    {
        var result = await _mediator.Send(new GetBranchVisitAttachmentQuery
        {
            VisitId = visitId,
            AttachmentId = attachmentId
        }, CancellationToken);
        if (result.IsFailed)
            return new BadRequestObjectResult(new { message = result.Error!.Value.Message, code = result.Error!.Value.Code });
        var (bytes, contentType, fileName) = result.Data;
        return File(bytes, contentType, fileName);
    }

    public sealed class OpenBody
    {
        public int BranchId { get; set; }
    }

    public sealed class RateBody
    {
        public int OrderListId { get; set; }
        public int Score { get; set; }
        public int ExpectedVersion { get; set; }
    }
}
