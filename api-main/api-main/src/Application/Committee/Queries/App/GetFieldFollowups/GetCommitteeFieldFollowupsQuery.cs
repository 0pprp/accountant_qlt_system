using Application.Committee.Common;
using Application.Common.Interfaces;
using Domain.Entities.FollowerWorkAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetFieldFollowups;

public record GetCommitteeFieldFollowupsQuery(int UserId)
    : IRequest<Result<List<CommitteeFieldFollowupDto>>>;

public class GetCommitteeFieldFollowupsHandler
    : IRequestHandler<GetCommitteeFieldFollowupsQuery, Result<List<CommitteeFieldFollowupDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetCommitteeFieldFollowupsHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<List<CommitteeFieldFollowupDto>>> Handle(
        GetCommitteeFieldFollowupsQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _db.FollowerFieldFollowups.AsNoTracking()
            .Where(x =>
                x.FollowerUserId == request.UserId
                && x.Status == FollowerFieldFollowup.StatusPending)
            .OrderBy(x => x.ListName)
            .ThenBy(x => x.CustomerName)
            .ThenBy(x => x.Id)
            .Select(x => new CommitteeFieldFollowupDto
            {
                Id = x.Id,
                FollowerUserId = x.FollowerUserId,
                ListId = x.ListId,
                ListName = x.ListName,
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                PhoneNumber = x.PhoneNumber,
                Status = x.Status,
                CompletionNote = x.CompletionNote,
                CreatedAtUtc = x.CreatedAtUtc,
                CompletedAtUtc = x.CompletedAtUtc
            })
            .ToListAsync(cancellationToken);

        return rows;
    }
}
