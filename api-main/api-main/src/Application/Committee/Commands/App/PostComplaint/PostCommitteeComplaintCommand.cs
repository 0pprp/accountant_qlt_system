using Application.Common.Interfaces;
using Application.Complaints.Commands;
using Application.Complaints.Common;

namespace Application.Committee.Commands.App.PostComplaint;

public record PostCommitteeComplaintCommand : IRequest<Result>
{
    public int UserId { get; set; }
    public required string Body { get; set; }
    public string? Subject { get; set; }
}

public class PostCommitteeComplaintHandler : IRequestHandler<PostCommitteeComplaintCommand, Result>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _current;

    public PostCommitteeComplaintHandler(IMediator mediator, ICurrentUserService current)
    {
        _mediator = mediator;
        _current = current;
    }

    public async Task<Result> Handle(PostCommitteeComplaintCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return Result.Failed(new Error("نص الشكوى إلزامي", "Complaint_Body_Required"));

        // الكاتب من الجلسة — لا نثق بـ UserId الجسم إن اختلف
        if (_current.UserId is not int sessionUser || sessionUser != request.UserId)
            return ComplaintErrors.Forbidden;

        var created = await _mediator.Send(new SubmitComplaintCommand
        {
            Body = request.Body,
            Subject = request.Subject,
            SourceApp = "committee"
        }, cancellationToken);

        return created.IsSucceeded ? Result.Success() : created.Error!;
    }
}
