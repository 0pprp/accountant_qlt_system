using Application.SalesRequests.Queries.App.Submitted;
using Application.SalesRequests.Commands.App.EnsureDraft;
using Application.SalesRequests.Commands.App.Submit;
using Application.SalesRequests.Commands.App.TransferName;
using Domain.Entities.SalesRequestAggregate.Enums;
using Application.SalesRequests.Commands.App.LinkDraft;
using Application.SalesRequests.Commands.App.Transition;
using Application.SalesRequests.Commands.App.View;
using Application.SalesRequests.Common;
using Application.SalesRequests.Queries.App.GetById;
using Application.SalesRequests.Queries.App.GetByTab;
using Application.SalesRequests.Queries.App.GetCounts;
using Application.SalesRequests.Queries.App.TransferPeers;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class SalesRequestsController : ApiController
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("submitted/{source}")]
    public async Task<ActionResult<SubmittedSalesRequestsPage>> Submitted(SalesRequestSourceType source,
        [FromQuery] string tab = "all", [FromQuery] int page = 1)
        => (await _mediator.Send(new GetSubmittedSalesRequestsQuery(source, tab, page), CancellationToken)).ToHttpResponse();

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("submission-branches/{source}")]
    public async Task<ActionResult<List<SubmissionBranch>>> SubmissionBranches(SalesRequestSourceType source)
        => (await _mediator.Send(new GetSubmissionBranchesQuery(source), CancellationToken)).ToHttpResponse();

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost("submit/{source}")]
    public async Task<ActionResult<SubmissionReceipt>> Submit(SalesRequestSourceType source, SubmitBody body)
        => (await _mediator.Send(new SubmitSalesRequestCommand {
            Source = source, SubmissionKey = body.SubmissionKey, BranchId = body.BranchId,
            CustomerName = body.CustomerName, CustomerPhone = body.CustomerPhone,
            Address = body.Address, RequestedProduct = body.RequestedProduct, Note = body.Note
        }, CancellationToken)).ToHttpResponse();

    public record SubmitBody(Guid SubmissionKey, int BranchId, string CustomerName, string CustomerPhone,
        string Address, string RequestedProduct, string? Note);

    private readonly IMediator _mediator;

    public SalesRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// تبويبات الموظف: incoming | prepared | inspected | pending | rejected | sold
    /// pending هنا = SalesRequest.Pending (معلقة) — ليس Order.WorkflowStatus.Pending.
    /// هوية القارئ من الجلسة فقط.
    /// </summary>
    [HasPermission(Permissions.SalesRequest.AppRead)]
    [HttpGet("counts")]
    public async Task<ActionResult<List<AppSalesRequestBinCount>>> Counts()
    {
        var result = await _mediator.Send(new GetAppSalesRequestCountsQuery(), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.AppRead)]
    [HttpGet]
    public async Task<ActionResult<List<SalesRequestAppItemDto>>> GetByTab([FromQuery] string tab)
    {
        var result = await _mediator.Send(new GetAppSalesRequestsByTabQuery { Tab = tab }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.AppRead)]
    [HttpGet("{requestId:int}")]
    public async Task<ActionResult<SalesRequestAppItemDto>> GetById([FromRoute] int requestId)
    {
        var result = await _mediator.Send(new GetAppSalesRequestByIdQuery(requestId), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.AppView)]
    [HttpPost("{requestId:int}/view")]
    public async Task<ActionResult> View([FromRoute] int requestId)
    {
        var result = await _mediator.Send(new ViewSalesRequestCommand(requestId), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.SalesRequest.AppPrepare)]
    [HttpPost("{requestId:int}/prepare")]
    public async Task<ActionResult> Prepare([FromRoute] int requestId, [FromBody] NoteBody? body)
    {
        return await Transition(requestId, EmployeeSalesRequestAction.Prepare, body?.Note);
    }

    [HasPermission(Permissions.SalesRequest.AppPend)]
    [HttpPost("{requestId:int}/pend")]
    public async Task<ActionResult> Pend([FromRoute] int requestId, [FromBody] NoteBody body)
    {
        return await Transition(requestId, EmployeeSalesRequestAction.Pend, body.Note);
    }

    [HasPermission(Permissions.SalesRequest.AppReject)]
    [HttpPost("{requestId:int}/reject")]
    public async Task<ActionResult> Reject([FromRoute] int requestId, [FromBody] NoteBody body)
    {
        return await Transition(requestId, EmployeeSalesRequestAction.Reject, body.Note);
    }

    [HasPermission(Permissions.SalesRequest.AppInspect)]
    [HttpPost("{requestId:int}/inspect")]
    public async Task<ActionResult> Inspect([FromRoute] int requestId)
    {
        return await Transition(requestId, EmployeeSalesRequestAction.Inspect, null);
    }

    [HasPermission(Permissions.SalesRequest.AppLinkDraft)]
    [HttpPost("{requestId:int}/link-draft")]
    public async Task<ActionResult<SalesRequestAppItemDto>> LinkDraft(
        [FromRoute] int requestId,
        [FromBody] LinkDraftBody body)
    {
        var result = await _mediator.Send(new LinkSalesRequestDraftCommand
        {
            RequestId = requestId,
            OrderId = body.OrderId
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>idempotent: يعيد LinkedOrderId الموجود أو ينشئ مسودة ويربطها إن وُجد زبون.</summary>
    [HasPermission(Permissions.SalesRequest.AppLinkDraft)]
    [HttpPost("{requestId:int}/ensure-draft")]
    public async Task<ActionResult<SalesRequestAppItemDto>> EnsureDraft([FromRoute] int requestId)
    {
        var result = await _mediator.Send(
            new EnsureSalesRequestDraftCommand(requestId), CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>عقد جديد: زملاء مؤهلون لنقل الاسم — الملكية من الجلسة وفرع الطلب.</summary>
    [HasPermission(Permissions.SalesRequest.AppTransferName)]
    [HttpGet("{requestId:int}/transfer-peers")]
    public async Task<ActionResult<List<TransferPeerItem>>> TransferPeers(
        [FromRoute] int requestId,
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(
            new GetTransferPeersQuery(requestId, search), CancellationToken);
        return result.ToHttpResponse();
    }

    /// <summary>عقد جديد: نقل الاسم لزميل — سبب إلزامي؛ يمنع المالك السابق من القراءة/الكتابة.</summary>
    [HasPermission(Permissions.SalesRequest.AppTransferName)]
    [HttpPost("{requestId:int}/transfer-name")]
    public async Task<ActionResult<SalesRequestAppItemDto>> TransferName(
        [FromRoute] int requestId,
        [FromBody] TransferNameBody body)
    {
        var result = await _mediator.Send(new TransferSalesRequestNameCommand
        {
            RequestId = requestId,
            ToEmployeeId = body.ToEmployeeId,
            Reason = body.Reason
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    private async Task<ActionResult> Transition(
        int requestId,
        EmployeeSalesRequestAction action,
        string? note)
    {
        var result = await _mediator.Send(new EmployeeSalesRequestTransitionCommand
        {
            RequestId = requestId,
            Action = action,
            Note = note
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    public record NoteBody(string? Note);
    public record LinkDraftBody(int OrderId);
    public record TransferNameBody(int ToEmployeeId, string Reason);
}
