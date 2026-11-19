using Application.Common.Interfaces;
using Domain.Entities.ArchiveAggregate;
using Domain.Entities.ArchiveAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Archive.Common;

/// <summary>
/// بناء كروت الحزم. العدّ يتمّ على الصفحة المعروضة وحدها — استعلام تجميع واحد
/// مقيّد بمعرّفات الصفحة ثم محوره في الذاكرة، لا تسعة استعلامات مرتبطة لكل صف.
/// </summary>
public static class ArchiveProjection
{
    public static async Task<List<ArchiveBundleListItemDto>> BuildAsync(
        IApplicationDbContext db,
        IReadOnlyList<ArchiveBundle> bundles,
        DateOnly baghdadToday,
        ArchiveStage? viewerStage,
        CancellationToken cancellationToken)
    {
        if (bundles.Count == 0)
            return [];

        var ids = bundles.Select(x => x.Id).ToList();

        var counts = await db.ArchiveTransactions
            .AsNoTracking()
            .Where(x => ids.Contains(x.BundleId))
            .GroupBy(x => new { x.BundleId, x.Status, x.IsVoided })
            .Select(g => new
            {
                g.Key.BundleId,
                g.Key.Status,
                g.Key.IsVoided,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var openReturns = await db.ArchiveTransactions
            .AsNoTracking()
            .Where(x => ids.Contains(x.BundleId) && x.HasOpenReturn && x.IsVoided == false)
            .GroupBy(x => x.BundleId)
            .Select(g => new { BundleId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var lastArchived = await db.ArchiveTransactions
            .AsNoTracking()
            .Where(x => ids.Contains(x.BundleId) && x.ArchivedAtUtc != null)
            .GroupBy(x => x.BundleId)
            .Select(g => new { BundleId = g.Key, Last = g.Max(x => x.ArchivedAtUtc) })
            .ToListAsync(cancellationToken);

        var branchNames = await db.Branches.AsNoTracking()
            .Where(b => bundles.Select(x => x.BranchId).Contains(b.Id))
            .Select(b => new { b.Id, b.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var result = new List<ArchiveBundleListItemDto>(bundles.Count);

        foreach (var bundle in bundles)
        {
            var mine = counts.Where(c => c.BundleId == bundle.Id).ToList();

            int Live(ArchiveTransactionStatus status) =>
                mine.Where(c => c.IsVoided == false && c.Status == status).Sum(c => c.Count);

            var voided = mine.Where(c => c.IsVoided).Sum(c => c.Count);
            var total = mine.Where(c => c.IsVoided == false).Sum(c => c.Count);
            var archived = Live(ArchiveTransactionStatus.Archived);
            var withEmployee = Live(ArchiveTransactionStatus.WithSalesEmployee);

            var isClosed = bundle.Period().IsClosedOn(baghdadToday);

            // الأرشيف نهاية المسار فلا يرسل، ولا شيء «بعهدته» ينتظر تمريراً.
            var pendingSend = viewerStage switch
            {
                ArchiveStage.SalesEmployee => withEmployee,
                ArchiveStage.BranchManagement => Live(ArchiveTransactionStatus.AtBranch),
                ArchiveStage.Follower => Live(ArchiveTransactionStatus.AtFollower),
                _ => 0
            };

            // «استلمها الطرف التالي» = ما وصل عهدته أو تجاوزها. الاسترجاع يُنقص العدد
            // عمداً، لأن الورق رجع فعلاً.
            var atBranchOrBeyond = Live(ArchiveTransactionStatus.AtBranch)
                                   + Live(ArchiveTransactionStatus.SentToFollower)
                                   + Live(ArchiveTransactionStatus.AtFollower)
                                   + Live(ArchiveTransactionStatus.SentToArchive)
                                   + archived;

            var atFollowerOrBeyond = Live(ArchiveTransactionStatus.AtFollower)
                                     + Live(ArchiveTransactionStatus.SentToArchive)
                                     + archived;

            // «تم استلام x من y» التي يراها المُرسِل: كم استلم منه الطرف التالي.
            var receivedOnward = viewerStage switch
            {
                ArchiveStage.BranchManagement => atFollowerOrBeyond,
                ArchiveStage.Follower => archived,
                ArchiveStage.Archive => archived,
                _ => atBranchOrBeyond
            };

            // وكم استلمتُ أنا من الوارد — رقم آخر، للمستلِم لا للمُرسِل.
            var receivedByMe = viewerStage switch
            {
                ArchiveStage.BranchManagement => atBranchOrBeyond,
                ArchiveStage.Follower => atFollowerOrBeyond,
                ArchiveStage.Archive => archived,
                _ => 0
            };

            result.Add(new ArchiveBundleListItemDto
            {
                Id = bundle.Id,
                BranchId = bundle.BranchId,
                BranchName = branchNames.GetValueOrDefault(bundle.BranchId),
                SalesEmployeeUserId = bundle.SalesEmployeeUserId,
                SalesEmployeeName = bundle.SalesEmployeeName,
                PeriodYear = bundle.PeriodYear,
                PeriodMonth = bundle.PeriodMonth,
                PeriodIndex = bundle.PeriodIndex,
                PeriodStart = bundle.PeriodStart,
                PeriodEnd = bundle.PeriodEnd,
                PeriodLabel = ArchiveLabels.Period(bundle.PeriodYear, bundle.PeriodMonth, bundle.PeriodIndex),
                IsPeriodClosed = isClosed,
                CanSend = pendingSend > 0
                          && viewerStage != ArchiveStage.Archive
                          && (viewerStage != ArchiveStage.SalesEmployee || isClosed),
                PendingSendCount = pendingSend,
                ItemCount = total,
                WithSalesEmployeeCount = withEmployee,
                SentToBranchCount = Live(ArchiveTransactionStatus.SentToBranch),
                AtBranchCount = Live(ArchiveTransactionStatus.AtBranch),
                SentToFollowerCount = Live(ArchiveTransactionStatus.SentToFollower),
                AtFollowerCount = Live(ArchiveTransactionStatus.AtFollower),
                SentToArchiveCount = Live(ArchiveTransactionStatus.SentToArchive),
                ArchivedCount = archived,
                VoidedCount = voided,
                OpenReturnCount = openReturns.FirstOrDefault(x => x.BundleId == bundle.Id)?.Count ?? 0,
                ReceivedOnwardCount = Math.Max(0, receivedOnward),
                ReceivedByMeCount = receivedByMe,
                FirstSentAtUtc = bundle.FirstSentAtUtc,
                LastArchivedAtUtc = lastArchived.FirstOrDefault(x => x.BundleId == bundle.Id)?.Last,
                ConcurrencyStamp = bundle.ConcurrencyStamp
            });
        }

        return result;
    }
}
