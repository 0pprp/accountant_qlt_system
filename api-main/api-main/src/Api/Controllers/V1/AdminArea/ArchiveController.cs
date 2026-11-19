using Application.Archive.Commands.Reconcile;
using Application.Archive.Commands.ReceiveTransactions;
using Application.Archive.Commands.ReturnTransactions;
using Application.Archive.Commands.SendBundle;
using Application.Archive.Commands.UndoSend;
using Application.Archive.Common;
using Application.Archive.Queries.GetBundle;
using Application.Archive.Queries.GetFlowBoard;
using Application.Archive.Queries.ListBundles;
using Application.Common.Models;
using Domain.Entities.ArchiveAggregate.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ArchiveController : ApiController
{
    private readonly IMediator _mediator;

    public ArchiveController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Archive.Read)]
    [HttpGet("bundles")]
    public async Task<ActionResult<PaginatedList<ArchiveBundleListItemDto>>> Bundles(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? branchId = null,
        [FromQuery] ArchiveStage? stage = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] bool pendingOnly = false)
    {
        var result = await _mediator.Send(
            new ListArchiveBundlesQuery
            {
                Pagination = new Pagination(pageIndex, pageSize),
                BranchId = branchId,
                Stage = stage,
                Year = year,
                Month = month,
                PendingOnly = pendingOnly
            },
            CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Archive.Read)]
    [HttpGet("bundles/{id:int}")]
    public async Task<ActionResult<ArchiveBundleDetailDto>> Bundle(
        [FromRoute] int id,
        [FromQuery] ArchiveStage? stage = null)
    {
        var result = await _mediator.Send(
            new GetArchiveBundleQuery { BundleId = id, Stage = stage },
            CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Archive.Send)]
    [HttpPost("bundles/{id:int}/send")]
    public async Task<ActionResult<ArchiveActionResultDto>> Send(
        [FromRoute] int id,
        [FromBody] SendArchiveBundleRequest body)
    {
        var result = await _mediator.Send(
            new SendArchiveBundleCommand
            {
                BundleId = id,
                FromStage = body.FromStage,
                Note = body.Note
            },
            CancellationToken);

        return result.ToHttpResponse();
    }

    [HttpPost("transactions/receive")]
    public async Task<ActionResult<ArchiveActionResultDto>> Receive(
        [FromBody] ReceiveArchiveTransactionsCommand body)
        => (await _mediator.Send(body, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Archive.UndoSend)]
    [HttpPost("transactions/undo-send")]
    public async Task<ActionResult<ArchiveActionResultDto>> UndoSend(
        [FromBody] UndoArchiveSendCommand body)
        => (await _mediator.Send(body, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Archive.Return)]
    [HttpPost("transactions/return")]
    public async Task<ActionResult<ArchiveActionResultDto>> Return(
        [FromBody] ReturnArchiveTransactionsCommand body)
        => (await _mediator.Send(body, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Archive.Oversight)]
    [HttpGet("flow-board")]
    public async Task<ActionResult<List<ArchiveFlowRowDto>>> FlowBoard(
        [FromQuery] int? branchId = null,
        [FromQuery] int limit = 50)
        => (await _mediator.Send(
                new GetArchiveFlowBoardQuery { BranchId = branchId, Limit = limit },
                CancellationToken))
            .ToHttpResponse();

    [HasPermission(Permissions.Archive.Reconcile)]
    [HttpPost("reconcile")]
    public async Task<ActionResult<ArchiveReconcileReport>> Reconcile()
        => (await _mediator.Send(new ReconcileArchiveCommand(), CancellationToken)).ToHttpResponse();
}

public record SendArchiveBundleRequest
{
    public ArchiveStage FromStage { get; init; }
    public string? Note { get; init; }
}
