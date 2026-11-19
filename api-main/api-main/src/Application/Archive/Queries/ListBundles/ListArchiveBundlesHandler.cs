using Application.SalesRequests.Common;
using Application.Archive.Common;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Archive.Queries.ListBundles;

public class ListArchiveBundlesHandler
    : IRequestHandler<ListArchiveBundlesQuery, Result<PaginatedList<ArchiveBundleListItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public ListArchiveBundlesHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<PaginatedList<ArchiveBundleListItemDto>>> Handle(
        ListArchiveBundlesQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.Archive.Read) == false)
            return ArchiveErrors.ActionForbidden;

        var actorId = _currentUser.UserId ?? 0;
        var branches = ArchiveAccess.ResolveBranchScope(_currentUser, request.BranchId);
        var empty = new PaginatedList<ArchiveBundleListItemDto>(
            [], 0, request.Pagination.PageIndex, request.Pagination.PageSize);

        if (branches.Count == 0 || actorId <= 0)
            return empty;

        var stage = request.Stage ?? ArchiveAccess.DefaultStageFor(_currentUser);
        if (stage is null)
            return empty;

        var query = _db.ArchiveBundles.AsNoTracking()
            .Where(x => branches.Contains(x.BranchId));

        if (stage == ArchiveStage.SalesEmployee && ArchiveAccess.HasOversight(_currentUser) == false)
            query = query.Where(x => x.SalesEmployeeUserId == actorId);

        if (request.Year is > 0)
            query = query.Where(x => x.PeriodYear == request.Year);

        if (request.Month is > 0)
            query = query.Where(x => x.PeriodMonth == request.Month);

        if (request.PendingOnly)
        {
            var awaited = WaitingStatus(stage.Value);
            query = query.Where(x => x.Transactions.Any(t => t.IsVoided == false && t.Status == awaited));
        }

        var total = await query.CountAsync(cancellationToken);

        var page = await query
            .OrderByDescending(x => x.PeriodYear)
            .ThenByDescending(x => x.PeriodMonth)
            .ThenByDescending(x => x.PeriodIndex)
            .ThenBy(x => x.SalesEmployeeName)
            .Skip((request.Pagination.PageIndex - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var today = ArchiveIntake.ToBaghdadDate(_clock.UtcNowOffset);
        var items = await ArchiveProjection.BuildAsync(_db, page, today, stage, cancellationToken);

        return new PaginatedList<ArchiveBundleListItemDto>(
            items, total, request.Pagination.PageIndex, request.Pagination.PageSize);
    }

    private static ArchiveTransactionStatus WaitingStatus(ArchiveStage stage) => stage switch
    {
        ArchiveStage.BranchManagement => ArchiveTransactionStatus.SentToBranch,
        ArchiveStage.Follower => ArchiveTransactionStatus.SentToFollower,
        ArchiveStage.Archive => ArchiveTransactionStatus.SentToArchive,
        _ => ArchiveTransactionStatus.WithSalesEmployee
    };
}
