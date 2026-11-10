using Application.AppNotes.Commands.MarkRead;
using Application.AppNotes.Common;
using Application.AppNotes.Queries;

namespace Api.Controllers.V1.AdminArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/admin/AppNotes")]
public class AppNotesController : ApiController
{
    private readonly IMediator _mediator;

    public AppNotesController(IMediator mediator) => _mediator = mediator;

    [HasPermission(Permissions.AppNote.Read)]
    [HttpGet]
    public async Task<ActionResult<AppNoteListResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int? branchId = null,
        [FromQuery] string? sourceApp = null,
        [FromQuery] string? search = null)
        => (await _mediator.Send(new ListAppNotesQuery
        {
            Page = page,
            PageSize = pageSize,
            UnreadOnly = unreadOnly,
            BranchId = branchId,
            SourceApp = sourceApp,
            Search = search
        }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.AppNote.Read)]
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> UnreadCount()
        => (await _mediator.Send(new GetAppNoteUnreadCountQuery(), CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.AppNote.Read)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppNoteDetailDto>> GetById([FromRoute] int id)
        => (await _mediator.Send(new GetAppNoteByIdQuery { Id = id }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.AppNote.Read)]
    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<AppNoteDetailDto>> MarkRead([FromRoute] int id)
        => (await _mediator.Send(new MarkAppNoteReadCommand { Id = id }, CancellationToken)).ToHttpResponse();

    [HasPermission(Permissions.AppNote.Read)]
    [HttpPut("read-all")]
    public async Task<ActionResult<int>> MarkAllRead()
        => (await _mediator.Send(new MarkAllAppNotesReadCommand(), CancellationToken)).ToHttpResponse();
}
