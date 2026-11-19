using Application.SalesRequests.Common;
using Application.Archive.Common;
using Application.Common.Interfaces;
using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Archive.Queries.GetBundle;

public class GetArchiveBundleHandler
    : IRequestHandler<GetArchiveBundleQuery, Result<ArchiveBundleDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetArchiveBundleHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<ArchiveBundleDetailDto>> Handle(
        GetArchiveBundleQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.Archive.Read) == false)
            return ArchiveErrors.ActionForbidden;

        var bundle = await _db.ArchiveBundles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.BundleId, cancellationToken);

        if (bundle is null)
            return ArchiveErrors.BundleNotFound;

        if (ArchiveAccess.CanAccessBranch(_currentUser, bundle.BranchId) == false)
            return ArchiveErrors.BranchForbidden;

        var actorId = _currentUser.UserId ?? 0;
        var stage = request.Stage ?? ArchiveAccess.DefaultStageFor(_currentUser);

        if (stage == ArchiveStage.SalesEmployee
            && ArchiveAccess.HasOversight(_currentUser) == false
            && bundle.SalesEmployeeUserId != actorId)
        {
            return ArchiveErrors.NotBundleOwner;
        }

        var rows = await _db.ArchiveTransactions.AsNoTracking()
            .Where(x => x.BundleId == bundle.Id)
            .Select(x => new
            {
                x.Id,
                x.OrderId,
                x.CustomerId,
                x.CustomerName,
                x.CustomerPhone,
                x.PeriodSourceDate,
                x.Status,
                x.IsVoided,
                x.VoidReason,
                x.HasOpenReturn,
                x.ReturnCount,
                x.LastReturnReason,
                x.FollowerUserId,
                FollowerName = x.Follower != null ? x.Follower.FullName : null,
                x.ConcurrencyStamp,
                x.SentToBranchAtUtc,
                x.ReceivedByBranchAtUtc,
                x.SentToFollowerAtUtc,
                x.ReceivedByFollowerAtUtc,
                x.SentToArchiveAtUtc,
                x.ArchivedAtUtc
            })
            .ToListAsync(cancellationToken);

        var awaiting = stage is null ? null : ArchiveTransaction.ReceiveSourceStatus(stage.Value);

        var transactions = rows
            .Select(x => new ArchiveTransactionDto
            {
                Id = x.Id,
                OrderId = x.OrderId,
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                CustomerPhone = x.CustomerPhone,
                PeriodSourceDate = x.PeriodSourceDate,
                Status = x.Status,
                StatusLabel = ArchiveLabels.Status(x.Status),
                HoldingStage = ArchiveLabels.HoldingStage(x.Status),
                IsInTransit = x.Status is ArchiveTransactionStatus.SentToBranch
                    or ArchiveTransactionStatus.SentToFollower
                    or ArchiveTransactionStatus.SentToArchive,
                IsVoided = x.IsVoided,
                VoidReason = x.VoidReason,
                HasOpenReturn = x.HasOpenReturn,
                ReturnCount = x.ReturnCount,
                LastReturnReason = x.LastReturnReason,
                FollowerUserId = x.FollowerUserId,
                FollowerName = x.FollowerName,
                ConcurrencyStamp = x.ConcurrencyStamp,
                SentToBranchAtUtc = x.SentToBranchAtUtc,
                ReceivedByBranchAtUtc = x.ReceivedByBranchAtUtc,
                SentToFollowerAtUtc = x.SentToFollowerAtUtc,
                ReceivedByFollowerAtUtc = x.ReceivedByFollowerAtUtc,
                SentToArchiveAtUtc = x.SentToArchiveAtUtc,
                ArchivedAtUtc = x.ArchivedAtUtc,
                AwaitsMyReceipt = awaiting is not null && x.Status == awaiting && x.IsVoided == false
            })
            // غير المستلمة تتصدّر، ثم المسترجَعة، ثم الباقي بالاسم.
            .OrderByDescending(x => x.AwaitsMyReceipt)
            .ThenByDescending(x => x.HasOpenReturn)
            .ThenBy(x => x.IsVoided)
            .ThenBy(x => x.CustomerName)
            .ToList();

        var today = ArchiveIntake.ToBaghdadDate(_clock.UtcNowOffset);
        var card = (await ArchiveProjection.BuildAsync(_db, [bundle], today, stage, cancellationToken))[0];

        var canSend = stage is not null
                      && ArchiveAccess.EnsureCanSend(_currentUser, stage.Value, bundle) is null
                      && card.CanSend;

        return new ArchiveBundleDetailDto
        {
            Bundle = card,
            Transactions = transactions,
            MyStage = stage,
            CanSend = canSend,
            CanReceive = stage is not null && ArchiveAccess.CanReceiveAt(_currentUser, stage.Value),
            CanReturn = _currentUser.HasPermission(SeedPermissions.Archive.Return),
            CanUndoSend = _currentUser.HasPermission(SeedPermissions.Archive.UndoSend)
        };
    }
}
