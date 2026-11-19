using Application.Archive.Common;
using Application.Common.Interfaces;
using Domain.Entities.ArchiveAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Archive.Queries.GetFlowBoard;

public class GetArchiveFlowBoardHandler
    : IRequestHandler<GetArchiveFlowBoardQuery, Result<List<ArchiveFlowRowDto>>>
{
    private sealed record FlowEvent(
        int TransactionId,
        int BundleId,
        ArchiveEventType EventType,
        ArchiveStage? Stage,
        DateTimeOffset AtUtc);

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetArchiveFlowBoardHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<List<ArchiveFlowRowDto>>> Handle(
        GetArchiveFlowBoardQuery request,
        CancellationToken cancellationToken)
    {
        if (ArchiveAccess.HasOversight(_currentUser) == false)
            return ArchiveErrors.ActionForbidden;

        var branches = ArchiveAccess.ResolveBranchScope(_currentUser, request.BranchId);
        if (branches.Count == 0)
            return new List<ArchiveFlowRowDto>();

        var limit = Math.Clamp(request.Limit, 1, 200);

        var bundles = await _db.ArchiveBundles.AsNoTracking()
            .Where(x => branches.Contains(x.BranchId))
            .OrderByDescending(x => x.PeriodYear)
            .ThenByDescending(x => x.PeriodMonth)
            .ThenByDescending(x => x.PeriodIndex)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (bundles.Count == 0)
            return new List<ArchiveFlowRowDto>();

        var ids = bundles.Select(x => x.Id).ToList();

        var branchNames = await _db.Branches.AsNoTracking()
            .Where(b => branches.Contains(b.Id))
            .Select(b => new { b.Id, b.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        // السرعة تُقاس من السجل الإلحاقي لا من الأختام: الاسترجاع يُمرّ المعاملة
        // بالمرحلة الواحدة أكثر من مرة فتُكتب الأختام فوق بعضها.
        var events = await (
            from e in _db.ArchiveTransactionEvents.AsNoTracking()
            join t in _db.ArchiveTransactions.AsNoTracking() on e.TransactionId equals t.Id
            where ids.Contains(t.BundleId)
            select new FlowEvent(e.TransactionId, t.BundleId, e.EventType, e.Stage, e.AtUtc)
        ).ToListAsync(cancellationToken);

        var states = await _db.ArchiveTransactions.AsNoTracking()
            .Where(t => ids.Contains(t.BundleId) && t.IsVoided == false)
            .Select(t => new { t.BundleId, t.Status, t.HasOpenReturn })
            .ToListAsync(cancellationToken);

        var now = _clock.UtcNowOffset;
        var rows = new List<ArchiveFlowRowDto>(bundles.Count);

        foreach (var bundle in bundles)
        {
            var mine = states.Where(x => x.BundleId == bundle.Id).ToList();
            var total = mine.Count;
            var archived = mine.Count(x => x.Status == ArchiveTransactionStatus.Archived);
            var bundleEvents = events.Where(x => x.BundleId == bundle.Id).ToList();

            rows.Add(new ArchiveFlowRowDto
            {
                BundleId = bundle.Id,
                SalesEmployeeName = bundle.SalesEmployeeName,
                BranchName = branchNames.GetValueOrDefault(bundle.BranchId),
                PeriodLabel = ArchiveLabels.Period(bundle.PeriodYear, bundle.PeriodMonth, bundle.PeriodIndex),
                ItemCount = total,
                ArchivedCount = archived,
                OpenReturnCount = mine.Count(x => x.HasOpenReturn),
                WhereLabel = ArchiveLabels.Where(bundle, archived, total),
                StuckAtStage = StuckAt(mine.Select(x => x.Status)),
                DaysSinceFirstSend = bundle.FirstSentAtUtc is { } first
                    ? (int)Math.Floor((now - first).TotalDays)
                    : null,
                AverageHoursSalesToBranch = AverageHours(
                    bundleEvents, ArchiveStage.SalesEmployee, ArchiveStage.BranchManagement),
                AverageHoursBranchToFollower = AverageHours(
                    bundleEvents, ArchiveStage.BranchManagement, ArchiveStage.Follower),
                AverageHoursFollowerToArchive = AverageHours(
                    bundleEvents, ArchiveStage.Follower, ArchiveStage.Archive)
            });
        }

        return rows;
    }

    private static ArchiveStage? StuckAt(IEnumerable<ArchiveTransactionStatus> statuses)
    {
        var list = statuses.ToList();
        if (list.Count == 0 || list.All(x => x == ArchiveTransactionStatus.Archived))
            return null;

        var earliest = list.Where(x => x != ArchiveTransactionStatus.Archived).Min();
        return ArchiveLabels.HoldingStage(earliest);
    }

    /// <summary>متوسط ساعات العبور: لكل استلام، آخر إرسال سبقه لنفس المعاملة.</summary>
    private static double? AverageHours(List<FlowEvent> events, ArchiveStage from, ArchiveStage to)
    {
        var sends = events
            .Where(e => e.EventType == ArchiveEventType.Sent && e.Stage == from)
            .ToList();

        var receipts = events
            .Where(e => e.EventType == ArchiveEventType.Received && e.Stage == to)
            .ToList();

        var spans = new List<double>();

        foreach (var receipt in receipts)
        {
            var sentAt = sends
                .Where(s => s.TransactionId == receipt.TransactionId && s.AtUtc <= receipt.AtUtc)
                .OrderByDescending(s => s.AtUtc)
                .Select(s => (DateTimeOffset?)s.AtUtc)
                .FirstOrDefault();

            if (sentAt is { } moment)
                spans.Add((receipt.AtUtc - moment).TotalHours);
        }

        return spans.Count == 0 ? null : Math.Round(spans.Average(), 1);
    }
}
