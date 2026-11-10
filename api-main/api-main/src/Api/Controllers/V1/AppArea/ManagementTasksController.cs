using Application.Common.Models;
using Application.ManagementTasks.Commands.Admin.Accept;
using Application.ManagementTasks.Commands.Admin.Complete;
using Application.ManagementTasks.Commands.Admin.Confirm;
using Application.ManagementTasks.Commands.Admin.Reject;
using Application.ManagementTasks.Commands.Admin.Transfer;
using Application.ManagementTasks.Common;
using Application.ManagementTasks.Queries.Admin.DownloadAttachment;
using Application.ManagementTasks.Queries.Admin.GetById;
using Application.ManagementTasks.Queries.App.Inbox;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class ManagementTasksController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public ManagementTasksController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("inbox")]
    public async Task<ActionResult<PaginatedList<ManagementTaskListItemDto>>> Inbox(
        [FromQuery] Pagination pagination,
        [FromQuery] ManagementTaskStatus? status)
    {
        var result = await _mediator.Send(new GetAppManagementTasksInboxQuery
        {
            Pagination = pagination,
            Status = status
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ManagementTaskDetailDto>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetManagementTaskByIdQuery(id), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementTask.Accept)]
    [HttpPost("{id:int}/accept")]
    public async Task<ActionResult<ManagementTaskActionResultDto>> Accept(
        [FromRoute] int id,
        [FromBody] ExpectedVersionBody body)
    {
        var result = await _mediator.Send(new AcceptManagementTaskCommand
        {
            TaskId = id,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementTask.Reject)]
    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<ManagementTaskActionResultDto>> Reject(
        [FromRoute] int id,
        [FromBody] RejectBody body)
    {
        var result = await _mediator.Send(new RejectManagementTaskCommand
        {
            TaskId = id,
            ExpectedVersion = body.ExpectedVersion,
            Note = body.Note
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementTask.Complete)]
    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<ManagementTaskActionResultDto>> Complete(
        [FromRoute] int id,
        [FromBody] ExpectedVersionBody body)
    {
        var result = await _mediator.Send(new CompleteManagementTaskCommand
        {
            TaskId = id,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementTask.Transfer)]
    [HttpPost("{id:int}/transfer")]
    public async Task<ActionResult<ManagementTaskActionResultDto>> Transfer(
        [FromRoute] int id,
        [FromBody] TransferBody body)
    {
        var result = await _mediator.Send(new TransferManagementTaskCommand
        {
            TaskId = id,
            ExpectedVersion = body.ExpectedVersion,
            TargetUserId = body.TargetUserId,
            Note = body.Note
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementTask.Confirm)]
    [HttpPost("{id:int}/confirm")]
    public async Task<ActionResult<ManagementTaskActionResultDto>> Confirm(
        [FromRoute] int id,
        [FromBody] ExpectedVersionBody body)
    {
        var result = await _mediator.Send(new ConfirmManagementTaskCommand
        {
            TaskId = id,
            ExpectedVersion = body.ExpectedVersion
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementTask.ReadAttachment)]
    [HttpGet("{id:int}/attachments/{fileId:int}")]
    public async Task<IActionResult> DownloadAttachment([FromRoute] int id, [FromRoute] int fileId)
    {
        var result = await _mediator.Send(new DownloadManagementTaskAttachmentQuery(id, fileId), CancellationToken);
        if (result.IsFailed)
            return result.ToHttpResponse().Result!;

        var dto = result.Data!;
        var fullPath = Path.Combine(_env.WebRootPath, dto.RelativePath);
        if (System.IO.File.Exists(fullPath) == false)
            return NotFound();

        return PhysicalFile(fullPath, dto.ContentType, dto.FileName);
    }

    public record ExpectedVersionBody(int ExpectedVersion);
    public record RejectBody(int ExpectedVersion, string Note);
    public record TransferBody(int ExpectedVersion, int TargetUserId, string? Note);
}
