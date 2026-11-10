using Application.AppNotes.Commands.Create;
using Application.AppNotes.Common;

namespace Api.Controllers.V1.AppArea;

/// <summary>إنشاء ملاحظة تطبيقات من الميدان — المصدر من الجلسة+التكليف لا من الجسم.</summary>
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/AppNotes")]
public class AppNotesController : ApiController
{
    private readonly IMediator _mediator;

    public AppNotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.AppNote.Create)]
    [HttpPost]
    public async Task<ActionResult<AppNoteDetailDto>> Create([FromBody] CreateAppNoteBody? body)
    {
        body ??= new CreateAppNoteBody();
        return (await _mediator.Send(new CreateAppNoteCommand
        {
            Body = body.Body ?? "",
            Subject = body.Subject
        }, CancellationToken)).ToHttpResponse();
    }

    public sealed class CreateAppNoteBody
    {
        public string? Body { get; set; }
        public string? Subject { get; set; }
    }
}
