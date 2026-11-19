using Api.Models;
using Application.Common;
using Application.Waivers.Commands.Create;
using Application.Waivers.Commands.Decide;
using Application.Waivers.Common;
using Application.Waivers.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/Waivers")]
public class WaiversController : ApiController
{
    private readonly IMediator _mediator;

    public WaiversController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.Waiver.Read)]
    [HttpGet]
    public async Task<ActionResult<WaiverListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] int? branchId = null,
        [FromQuery] string? search = null,
        [FromQuery] bool mineOnly = false)
        => (await _mediator.Send(new ListWaiversQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            BranchId = branchId,
            Search = search,
            MineOnly = mineOnly
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Waiver.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<WaiverDetailDto>> GetById([FromRoute] int id)
        => (await _mediator.Send(new GetWaiverByIdQuery { Id = id }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Waiver.Create)]
    [HttpPost]
    public async Task<ActionResult<WaiverDetailDto>> Create([FromBody] CreateBody body)
        => (await _mediator.Send(new CreateWaiverRequestCommand
        {
            CustomerId = body.CustomerId,
            Note = body.Note ?? "",
            OrderId = body.OrderId
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Waiver.Decide)]
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<WaiverDetailDto>> Approve([FromRoute] int id, [FromBody] ApproveBody body)
        => ToDecisionResponse(await _mediator.Send(new ApproveWaiverByDelegateCommand
        {
            Id = id,
            ExpectedVersion = body.ExpectedVersion,
            ApprovedAmount = body.ApprovedAmount,
            Note = body.Note
        }, CancellationToken));

    [HasPermission(Permissions.Waiver.Decide)]
    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<WaiverDetailDto>> Reject([FromRoute] int id, [FromBody] RejectBody body)
        => ToDecisionResponse(await _mediator.Send(new RejectWaiverCommand
        {
            Id = id,
            ExpectedVersion = body.ExpectedVersion,
            Note = body.Note
        }, CancellationToken));

    [HasPermission(Permissions.Waiver.Apply)]
    [HttpPost("{id:int}/apply")]
    public async Task<ActionResult<WaiverDetailDto>> Apply([FromRoute] int id, [FromBody] ApplyBody body)
        => ToDecisionResponse(await _mediator.Send(new ApplyWaiverByAccountantCommand
        {
            Id = id,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken));

    private static ActionResult<WaiverDetailDto> ToDecisionResponse(Result<WaiverDetailDto> result)
        => result.IsFailed
           && (result.Error?.Code == WaiverErrors.Concurrency.Code
               || result.Error?.Code == WaiverErrors.InvalidTransition.Code)
            ? new ConflictObjectResult(new BadRequestBody(result.Error))
            : result.ToHttpResponse();

    public sealed class CreateBody
    {
        public int CustomerId { get; set; }
        public string? Note { get; set; }
        public int? OrderId { get; set; }
    }

    public sealed class ApproveBody
    {
        public double ApprovedAmount { get; set; }
        public int ExpectedVersion { get; set; }
        public string? Note { get; set; }
    }

    public sealed class RejectBody
    {
        public int ExpectedVersion { get; set; }
        public string? Note { get; set; }
    }

    public sealed class ApplyBody
    {
        public int ExpectedVersion { get; set; }
    }
}
