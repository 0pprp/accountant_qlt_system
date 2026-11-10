using Application.Common.Models;
using Application.ManagementTasks.Commands.Admin.Accept;
using Application.ManagementTasks.Commands.Admin.AddAttachment;
using Application.ManagementTasks.Commands.Admin.Complete;
using Application.ManagementTasks.Commands.Admin.Confirm;
using Application.ManagementTasks.Commands.Admin.Create;
using Application.ManagementTasks.Commands.Admin.Reject;
using Application.ManagementTasks.Commands.Admin.Resend;
using Application.ManagementTasks.Commands.Admin.Transfer;
using Application.ManagementTasks.Common;
using Application.ManagementTasks.Queries.Admin.Assignees;
using Application.ManagementTasks.Queries.Admin.DownloadAttachment;
using Application.ManagementTasks.Queries.Admin.GetById;
using Application.ManagementTasks.Queries.Admin.List;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/[Controller]")]
public class ManagementTasksController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public ManagementTasksController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    /// <summary>
    /// القائمة الكاملة تتطلب Task.Read داخل الـhandler.
    /// InboxForMe=true يسمح بصلاحيات الإجراء دون Read (مهامي).
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<ManagementTaskListItemDto>>> List(
        [FromQuery] Pagination pagination,
        [FromQuery] int? branchId,
        [FromQuery] ManagementTaskStatus? status,
        [FromQuery] string? search,
        [FromQuery] bool inboxForMe = false)
    {
        var result = await _mediator.Send(new GetManagementTasksQuery
        {
            Pagination = pagination,
            BranchId = branchId,
            Status = status,
            Search = search,
            InboxForMe = inboxForMe
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementTask.Assign)]
    [HttpGet("assignee-options")]
    public async Task<ActionResult<List<ManagementTaskAssigneeOptionDto>>> AssigneeOptions(
        [FromQuery] int branchId)
    {
        var result = await _mediator.Send(new GetManagementTaskAssigneeOptionsQuery
        {
            BranchId = branchId
        }, CancellationToken);
        return result.ToHttpResponse();
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ManagementTaskDetailDto>> GetById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetManagementTaskByIdQuery(id), CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementTask.Create)]
    [HttpPost]
    public async Task<ActionResult<ManagementTaskActionResultDto>> Create(
        [FromBody] CreateManagementTaskCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);
        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.ManagementTask.Assign)]
    [HttpPost("{id:int}/resend")]
    public async Task<ActionResult<ManagementTaskActionResultDto>> Resend(
        [FromRoute] int id,
        [FromBody] ResendBody body)
    {
        var result = await _mediator.Send(new ResendManagementTaskCommand
        {
            TaskId = id,
            ExpectedVersion = body.ExpectedVersion,
            Title = body.Title,
            Description = body.Description,
            Priority = body.Priority,
            DueDate = body.DueDate,
            DueAtUtc = body.DueAtUtc,
            AssigneeUserId = body.AssigneeUserId,
            CustomerId = body.CustomerId,
            OrderId = body.OrderId
        }, CancellationToken);
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

    [HasPermission(Permissions.ManagementTask.Create)]
    [HttpPost("{id:int}/attachments")]
    [RequestSizeLimit(32 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 32 * 1024 * 1024)]
    public async Task<ActionResult<ManagementTaskAttachmentDto>> UploadAttachment(
        [FromRoute] int id,
        IFormFile? file)
    {
        var result = await _mediator.Send(new AddManagementTaskAttachmentCommand
        {
            TaskId = id,
            File = file!
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
    public record ResendBody(
        int ExpectedVersion,
        string Title,
        string Description,
        ManagementTaskPriority Priority,
        DateOnly DueDate,
        DateTimeOffset? DueAtUtc,
        int AssigneeUserId,
        int? CustomerId,
        int? OrderId);
}
