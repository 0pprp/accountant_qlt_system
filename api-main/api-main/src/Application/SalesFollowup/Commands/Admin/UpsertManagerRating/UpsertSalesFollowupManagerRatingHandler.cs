using Application.Common.Interfaces;
using Application.SalesFollowup.Common;
using Domain.Entities.SalesFollowupAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesFollowup.Commands.Admin.UpsertManagerRating;

public class UpsertSalesFollowupManagerRatingHandler
    : IRequestHandler<UpsertSalesFollowupManagerRatingCommand, Result<SalesFollowupManagerRatingDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public UpsertSalesFollowupManagerRatingHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SalesFollowupManagerRatingDto>> Handle(
        UpsertSalesFollowupManagerRatingCommand request,
        CancellationToken cancellationToken)
    {
        var branchError = SalesFollowupAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError;

        if (request.Score is < -10 or > 10)
            return SalesFollowupErrors.InvalidManagerScore;

        var now = _clock.UtcNowOffset;
        var guard = await SalesFollowupEmployeeGuard.EnsureAssignedInBranchAsync(
            _db, request.EmployeeId, request.BranchId, now, cancellationToken);
        if (guard is not null)
            return guard;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == _currentUser.UserId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        var row = new DirectorSalesEmployeeBoardRating(
            request.EmployeeId,
            request.Score,
            request.Note,
            _currentUser.UserId,
            actorName,
            _clock.UtcNowOffset);

        _db.DirectorSalesEmployeeBoardRatings.Add(row);
        await _db.SaveChangesAsync(cancellationToken);

        return new SalesFollowupManagerRatingDto
        {
            Score = row.Score,
            Note = row.Note,
            RatedAt = row.RatedAtUtc,
            RatedByUserName = row.RatedByUserName,
        };
    }
}
