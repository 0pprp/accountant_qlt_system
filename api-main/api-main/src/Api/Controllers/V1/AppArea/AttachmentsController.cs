using Application.Attachments.Commands.Common.Create;
using Application.Attachments.Commands.Common.Update;

namespace Api.Controllers.V1.AppArea;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/app/[Controller]")]
public class AttachmentsController : ApiController
{
    private readonly IMediator _mediator;

    public AttachmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission(Permissions.Attachment.Create)]
    [HttpPost]
    public async Task<ActionResult<CreateAttachmentResponse>> Create([FromForm] CreateAttachmentCommand command)
    {
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }

    [HasPermission(Permissions.Attachment.Update)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UpdateAttachmentResponse>> Update([FromRoute] int id, [FromForm] UpdateAttachmentDto dto)
    {
        var command = new UpdateAttachmentCommand
        {
            Id = id,
            File = dto.File
        };
        var result = await _mediator.Send(command, CancellationToken);

        return result.ToHttpResponse();
    }
}