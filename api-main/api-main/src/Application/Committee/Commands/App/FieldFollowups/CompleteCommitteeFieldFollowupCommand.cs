using Application.Committee.Common;
using Application.Common.Interfaces;
using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Commands.App.FieldFollowups;

public record CompleteCommitteeFieldFollowupCommand(
    int UserId,
    int Id,
    string? Note) : IRequest<Result<CommitteeFieldFollowupDto>>;

public class CompleteCommitteeFieldFollowupHandler
    : IRequestHandler<CompleteCommitteeFieldFollowupCommand, Result<CommitteeFieldFollowupDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public CompleteCommitteeFieldFollowupHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<CommitteeFieldFollowupDto>> Handle(
        CompleteCommitteeFieldFollowupCommand request,
        CancellationToken cancellationToken)
    {
        var note = FollowerFieldFollowup.NormalizeNote(request.Note);
        if (string.IsNullOrEmpty(note))
            return CommitteeFieldFollowupErrors.NoteRequired;

        var row = await _db.FollowerFieldFollowups
            .FirstOrDefaultAsync(
                x => x.Id == request.Id && x.FollowerUserId == request.UserId,
                cancellationToken);
        if (row is null)
            return CommitteeFieldFollowupErrors.NotFound;

        if (!row.IsPending)
            return CommitteeFieldFollowupErrors.AlreadyCompleted;

        try
        {
            row.Complete(note, _clock.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return new Error(ex.Message, "Committee_FieldFollowup_Invalid");
        }

        await _db.SaveChangesAsync(cancellationToken);
        return AddCommitteeFieldFollowupHandler.Map(row);
    }
}
