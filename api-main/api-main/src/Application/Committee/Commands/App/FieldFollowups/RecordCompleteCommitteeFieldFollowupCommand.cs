using Application.Committee.Common;
using Domain.Entities.FollowerWorkAggregate;

namespace Application.Committee.Commands.App.FieldFollowups;

/// <summary>Add (إن لزم) ثم Complete بملاحظة — مكافئ record-complete المرجع من كارت القائمة.</summary>
public record RecordCompleteCommitteeFieldFollowupCommand(
    int UserId,
    int ListId,
    string? ListName,
    int CustomerId,
    string? CustomerName,
    string? PhoneNumber,
    string? Note) : IRequest<Result<CommitteeFieldFollowupDto>>;

public class RecordCompleteCommitteeFieldFollowupHandler
    : IRequestHandler<RecordCompleteCommitteeFieldFollowupCommand, Result<CommitteeFieldFollowupDto>>
{
    private readonly IMediator _mediator;

    public RecordCompleteCommitteeFieldFollowupHandler(IMediator mediator) => _mediator = mediator;

    public async Task<Result<CommitteeFieldFollowupDto>> Handle(
        RecordCompleteCommitteeFieldFollowupCommand request,
        CancellationToken cancellationToken)
    {
        var note = FollowerFieldFollowup.NormalizeNote(request.Note);
        if (string.IsNullOrEmpty(note))
            return CommitteeFieldFollowupErrors.NoteRequired;

        var added = await _mediator.Send(
            new AddCommitteeFieldFollowupCommand(
                request.UserId,
                request.ListId,
                request.ListName,
                request.CustomerId,
                request.CustomerName,
                request.PhoneNumber),
            cancellationToken);
        if (!added.IsSucceeded || added.Data is null)
            return added;

        return await _mediator.Send(
            new CompleteCommitteeFieldFollowupCommand(request.UserId, added.Data.Id, note),
            cancellationToken);
    }
}
