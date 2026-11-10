using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.App.TransferPeers;

public sealed class GetTransferPeersHandler
    : IRequestHandler<GetTransferPeersQuery, Result<List<TransferPeerItem>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetTransferPeersHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<List<TransferPeerItem>>> Handle(
        GetTransferPeersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return SalesRequestErrors.NotOwner;

        var entity = await _db.SalesRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && !x.IsDeleted, cancellationToken);
        if (entity is null)
            return SalesRequestErrors.NotFound;
        if (!entity.IsOwnedBy(userId.Value))
            return SalesRequestErrors.NotOwner;

        var now = _clock.UtcNowOffset;
        var query = SalesRequestEmployeeEligibility.QueryEligibleAssignees(
                _db, entity.BranchId, now)
            .Where(x => x.Id != userId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                (x.FullName != null && x.FullName.Contains(term))
                || (x.Username != null && x.Username.Contains(term)));
        }

        return await query
            .OrderBy(x => x.FullName)
            .Take(50)
            .Select(x => new TransferPeerItem
            {
                Id = x.Id,
                FullName = x.FullName ?? x.Username ?? $"#{x.Id}",
                Username = x.Username
            })
            .ToListAsync(cancellationToken);
    }
}