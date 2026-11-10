using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;
using Domain.Entities.DelegateFollowupAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.DelegateFollowup.Commands.Admin.UpsertBoardOperatorRating;

public class UpsertBoardOperatorRatingHandler
    : IRequestHandler<UpsertBoardOperatorRatingCommand, Result<DelegateFollowupActionResultDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public UpsertBoardOperatorRatingHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<DelegateFollowupActionResultDto>> Handle(
        UpsertBoardOperatorRatingCommand request,
        CancellationToken cancellationToken)
    {
        var branchError = DelegateFollowupAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        if (DelegateFollowupRules.IsValidBoardOperatorScore(request.Score) == false)
            return DelegateFollowupErrors.InvalidBoardScore;

        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return DelegateFollowupErrors.ActorRequired;

        var listExists = await _db.OrderLists.AsNoTracking()
            .AnyAsync(l => l.Id == request.ListId && l.BranchId == request.BranchId, cancellationToken);
        if (listExists == false)
            return DelegateFollowupErrors.ListNotFound;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);
        var actorNameClean = string.IsNullOrWhiteSpace(actorName) ? null : actorName.Trim();

        var ratingDate = _clock.Today;
        var note = DelegateFollowupRules.NormalizeNote(request.Note);
        var at = _clock.UtcNowOffset;

        var existing = await _db.DirectorBoardOperatorListRatings
            .FirstOrDefaultAsync(x => x.OrderListId == request.ListId && x.RatingDate == ratingDate, cancellationToken);

        var created = existing is null;
        DirectorBoardOperatorListRating row;
        if (existing is null)
        {
            row = new DirectorBoardOperatorListRating(
                request.ListId,
                request.BranchId,
                ratingDate,
                request.Score,
                note,
                actorId,
                actorNameClean,
                at);
            _db.DirectorBoardOperatorListRatings.Add(row);
        }
        else
        {
            row = existing;
            row.Update(request.Score, note, actorId, actorNameClean, at);
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException) when (created)
        {
            _db.DirectorBoardOperatorListRatings.Remove(row);

            var concurrent = await _db.DirectorBoardOperatorListRatings
                .FirstOrDefaultAsync(
                    x => x.OrderListId == request.ListId && x.RatingDate == ratingDate,
                    cancellationToken);
            if (concurrent is null)
                throw;

            created = false;
            row = concurrent;
            row.Update(request.Score, note, actorId, actorNameClean, at);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new DelegateFollowupActionResultDto
        {
            Created = created,
            Rating = DelegateFollowupDataService.MapBoardRating(request.ListId, ratingDate, row)
        };
    }
}
