using Application.Common.Interfaces;
using Domain.Entities.ArchiveAggregate.Enums;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Archive.Common;

public record ArchiveReconcileReport
{
    public int Created { get; init; }
    public int Voided { get; init; }
    public int Revived { get; init; }
    public int BundlesTouched { get; init; }
    public int Scanned { get; init; }
}

/// <summary>
/// المرجع الذي يضمن تمام البيانات: الخطّافات تُسرّع الظهور، وهذا يُصلح ما فاتها.
/// يمشي في اتجاهين — يُدخل كل مبيع مُتمّ بلا معاملة، ويُبطِل معاملة كل مبيع غادر «مُتمّ».
/// </summary>
public class ArchiveReconciler
{
    public const int BatchSize = 200;

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public ArchiveReconciler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    /// <summary>
    /// <paramref name="since"/> أرضيةٌ للتشغيل الدوري حتى لا يمسح التاريخ كلّه كل دقيقة.
    /// تركه فارغاً يمسح كل شيء — وهو وضع التشغيل اليدوي.
    /// </summary>
    public async Task<ArchiveReconcileReport> RunAsync(
        DateTimeOffset? since,
        CancellationToken cancellationToken)
    {
        var at = _clock.UtcNowOffset;
        var created = 0;
        var voided = 0;
        var revived = 0;
        var scanned = 0;
        var bundles = new HashSet<int>();

        var missing = _db.Orders
            .Where(o => o.WorkflowStatus == SalesWorkflowStatus.Sold
                        && o.ApprovalStatus != OrderApprovalStatus.Rejected
                        && o.SellerId != null
                        && _db.ArchiveTransactions.Any(t => t.OrderId == o.Id) == false);

        if (since is { } floor)
            missing = missing.Where(o => o.SalesCompletedAtUtc == null || o.SalesCompletedAtUtc >= floor);

        var orders = await missing.OrderBy(o => o.Id).Take(BatchSize).ToListAsync(cancellationToken);
        scanned += orders.Count;

        foreach (var order in orders)
        {
            var transaction = await ArchiveIntake.EnsureAsync(_db, order, cancellationToken);
            if (transaction is null)
                continue;

            created++;
            bundles.Add(transaction.BundleId);
        }

        if (created > 0)
            await _db.SaveChangesAsync(cancellationToken);

        // الاتجاه الآخر: مبيع غادر «مُتمّ» ومعاملته ما زالت حيّة.
        var stale = await _db.ArchiveTransactions
            .Include(t => t.Bundle)
            .Where(t => t.IsVoided == false
                        && _db.Orders.Any(o => o.Id == t.OrderId
                                               && (o.WorkflowStatus != SalesWorkflowStatus.Sold
                                                   || o.ApprovalStatus == OrderApprovalStatus.Rejected)))
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var transaction in stale)
        {
            if (transaction.Void("المبيع لم يعد مُتمّاً", at) == false)
                continue;

            voided++;
            if (transaction.Bundle is not null)
                bundles.Add(transaction.Bundle.Id);
        }

        // ومبيع عاد إلى «مُتمّ» بعد إبطال.
        var returned = await _db.ArchiveTransactions
            .Include(t => t.Bundle)
            .Where(t => t.IsVoided
                        && _db.Orders.Any(o => o.Id == t.OrderId
                                               && o.WorkflowStatus == SalesWorkflowStatus.Sold
                                               && o.ApprovalStatus != OrderApprovalStatus.Rejected))
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var transaction in returned)
        {
            if (transaction.Revive() == false)
                continue;

            revived++;
            if (transaction.Bundle is not null)
                bundles.Add(transaction.Bundle.Id);
        }

        scanned += stale.Count + returned.Count;

        foreach (var bundleId in bundles)
        {
            var bundle = await _db.ArchiveBundles.FirstOrDefaultAsync(x => x.Id == bundleId, cancellationToken);
            if (bundle is not null)
                await ArchiveRecorder.RecomputeCountersAsync(_db, bundle, cancellationToken);
        }

        if (voided > 0 || revived > 0 || bundles.Count > 0)
            await _db.SaveChangesAsync(cancellationToken);

        return new ArchiveReconcileReport
        {
            Created = created,
            Voided = voided,
            Revived = revived,
            BundlesTouched = bundles.Count,
            Scanned = scanned
        };
    }
}
