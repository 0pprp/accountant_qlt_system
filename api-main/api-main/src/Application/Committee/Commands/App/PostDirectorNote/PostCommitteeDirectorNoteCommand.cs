using Application.AppNotes.Commands.Create;
using Application.AppNotes.Common;
using Domain.Entities.AppNoteAggregate.Enums;

namespace Application.Committee.Commands.App.PostDirectorNote;

/// <summary>
/// مسار توافق لتطبيق اللجنة — يخزّن في AppNotes بمصدر Follower.
/// لا يقبل هوية الكاتب من العميل.
/// </summary>
public record PostCommitteeDirectorNoteCommand : IRequest<Result<AppNoteDetailDto>>
{
    public required string Body { get; set; }
    public string? Subject { get; set; }
}

public class PostCommitteeDirectorNoteHandler
    : IRequestHandler<PostCommitteeDirectorNoteCommand, Result<AppNoteDetailDto>>
{
    private readonly IMediator _mediator;

    public PostCommitteeDirectorNoteHandler(IMediator mediator) => _mediator = mediator;

    public Task<Result<AppNoteDetailDto>> Handle(
        PostCommitteeDirectorNoteCommand request,
        CancellationToken cancellationToken)
        => _mediator.Send(new CreateAppNoteCommand
        {
            Body = request.Body,
            Subject = request.Subject,
            SourceAppOverride = AppNoteSourceApp.Follower
        }, cancellationToken);
}
