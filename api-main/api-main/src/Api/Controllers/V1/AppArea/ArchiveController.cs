using Application.Archive.Commands.ReceiveTransactions;
using Application.Archive.Commands.SendBundle;
using Application.Archive.Commands.UndoSend;
using Application.Archive.Common;
using Application.Archive.Queries.GetBundle;
using Application.Archive.Queries.ListBundles;
using Application.Common.Models;
using Domain.Entities.ArchiveAggregate.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.AppArea;

/// <summary>
/// الأرشيف لتطبيقي موظف المبيعات والمتابع. نفس المُعالِجات تماماً —
/// الاختلاف في المسار والشريحة لا في المنطق.
/// </summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
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
        [FromQuery] ArchiveStage? stage = null,
        [FromQuery] bool pendingOnly = false)
    {
        var result = await _mediator.Send(
            new ListArchiveBundlesQuery
            {
                Pagination = new Pagination(pageIndex, pageSize),
                Stage = stage,
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
        => (await _mediator.Send(
                new GetArchiveBundleQuery { BundleId = id, Stage = stage },
                CancellationToken))
            .ToHttpResponse();

    [HasPermission(Permissions.Archive.Send)]
    [HttpPost("bundles/{id:int}/send")]
    public async Task<ActionResult<ArchiveActionResultDto>> Send(
        [FromRoute] int id,
        [FromBody] AppSendArchiveBundleRequest body)
        => (await _mediator.Send(
                new SendArchiveBundleCommand
                {
                    BundleId = id,
                    FromStage = body.FromStage,
                    Note = body.Note
                },
                CancellationToken))
            .ToHttpResponse();

    [HttpPost("transactions/receive")]
    public async Task<ActionResult<ArchiveActionResultDto>> Receive(
        [FromBody] ReceiveArchiveTransactionsCommand body)
        => (await _mediator.Send(body, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.Archive.UndoSend)]
    [HttpPost("transactions/undo-send")]
    public async Task<ActionResult<ArchiveActionResultDto>> UndoSend(
        [FromBody] UndoArchiveSendCommand body)
        => (await _mediator.Send(body, CancellationToken)).ToHttpResponse();
}

public record AppSendArchiveBundleRequest
{
    public ArchiveStage FromStage { get; init; }
    public string? Note { get; init; }
}
