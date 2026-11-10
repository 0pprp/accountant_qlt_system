using Application.Committee.Common;
using Application.Common.Interfaces;
using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetFieldFollowups;

public record GetCommitteeFieldFollowupLatestNotesQuery(int UserId, int ListId)
    : IRequest<Result<List<CommitteeFieldFollowupLatestNoteDto>>>;

public class GetCommitteeFieldFollowupLatestNotesHandler
    : IRequestHandler<GetCommitteeFieldFollowupLatestNotesQuery,
        Result<List<CommitteeFieldFollowupLatestNoteDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetCommitteeFieldFollowupLatestNotesHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<List<CommitteeFieldFollowupLatestNoteDto>>> Handle(
        GetCommitteeFieldFollowupLatestNotesQuery request,
        CancellationToken cancellationToken)
    {
        if (!await CommitteeAccess.OwnsListAsync(_db, request.UserId, request.ListId, cancellationToken))
            return CommitteeErrors.ListNotFound;

        var rows = await _db.FollowerFieldFollowups.AsNoTracking()
            .Where(x =>
                x.FollowerUserId == request.UserId
                && x.ListId == request.ListId
                && x.Status == FollowerFieldFollowup.StatusCompleted
                && x.CompletionNote != null)
            .OrderByDescending(x => x.CompletedAtUtc)
            .ThenByDescending(x => x.Id)
            .Select(x => new { x.CustomerId, x.CompletionNote, x.CompletedAtUtc })
            .ToListAsync(cancellationToken);

        var latest = rows
            .GroupBy(x => x.CustomerId)
            .Select(g => g.First())
            .Select(x => new CommitteeFieldFollowupLatestNoteDto
            {
                CustomerId = x.CustomerId,
                Note = x.CompletionNote,
                CompletedAtUtc = x.CompletedAtUtc
            })
            .ToList();

        return latest;
    }
}
