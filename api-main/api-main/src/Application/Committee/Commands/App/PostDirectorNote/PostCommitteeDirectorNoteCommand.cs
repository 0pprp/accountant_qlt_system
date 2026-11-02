namespace Application.Committee.Commands.App.PostDirectorNote;

public record PostCommitteeDirectorNoteCommand : IRequest<Result>
{
    public int UserId { get; set; }
    public required string Body { get; set; }
}

public class PostCommitteeDirectorNoteHandler : IRequestHandler<PostCommitteeDirectorNoteCommand, Result>
{
    public Task<Result> Handle(PostCommitteeDirectorNoteCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return Task.FromResult(Result.Failed(new Error("نص الملاحظة إلزامي", "DirectorNote_Body_Required")));
        return Task.FromResult(Result.Success());
    }
}
