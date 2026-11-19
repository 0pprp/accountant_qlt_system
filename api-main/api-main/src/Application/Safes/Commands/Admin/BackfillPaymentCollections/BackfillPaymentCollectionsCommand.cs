using Application.Common.Interfaces;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Commands.Admin.BackfillPaymentCollections;

/// <summary>
/// ترحيل تسديدات النظام الجديد التي سبقت ربط التسديد بالقاصة. التسديدات المستوردة من النظام القديم
/// موجودة في القاصات أصلاً كحركات «تحصيل» مستوردة فلا تُرحَّل مرة ثانية.
/// </summary>
public record BackfillPaymentCollectionsCommand : IRequest<Result<BackfillPaymentCollectionsResponse>>
{
    public int BranchId { get; init; }
    public bool DryRun { get; init; } = true;
}

public sealed record BackfillListLineDto(int OrderListId, string OrderListName, int Count, double Amount);

public sealed record BackfillPaymentCollectionsResponse(
    bool DryRun,
    int Count,
    double Amount,
    IReadOnlyList<BackfillListLineDto> Lists,
    int SkippedImported,
    int SkippedNotEnteredAsPayment,
    int AlreadyPosted);

public class BackfillPaymentCollectionsHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    ISafeLedger ledger,
    IActivityLogService activityLog)
    : IRequestHandler<BackfillPaymentCollectionsCommand, Result<BackfillPaymentCollectionsResponse>>
{
    public async Task<Result<BackfillPaymentCollectionsResponse>> Handle(
        BackfillPaymentCollectionsCommand request, CancellationToken cancellationToken)
    {
        var scoped = SafeAccess.EnsureBranchInScope(currentUser, request.BranchId);
        if (scoped.IsFailed)
            return scoped.Error;

        var inBranch = db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId != null && p.Order!.OrderListId != null && p.Order.OrderList!.BranchId == request.BranchId);

        var alreadyPosted = await inBranch
            .CountAsync(p => db.SafeMovements.Any(m => m.InstallmentPaymentId == p.Id), cancellationToken);
        var skippedImported = await inBranch
            .CountAsync(p => p.ImportKey != null, cancellationToken);

        var unposted = inBranch
            .Where(p => p.ImportKey == null && db.SafeMovements.Any(m => m.InstallmentPaymentId == p.Id) == false);

        var candidates = await unposted
            .Where(p => db.ActivityLogs.Any(l =>
                l.TargetEntityType == TargetEntityType.InstallmentPayment
                && l.TargetEntityId == p.Id
                && (l.ActivityType == ActivityType.InstallmentPaymentCreated
                    || l.ActivityType == ActivityType.InstallmentPaymentsSynced)))
            .OrderBy(p => p.Id)
            .Select(p => new
            {
                p.Id,
                OrderId = p.OrderId!.Value,
                OrderListId = p.Order!.OrderListId!.Value,
                OrderListName = p.Order.OrderList!.Name,
                p.Amount,
                p.CreatedBy,
                p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var skippedNotEntered = await unposted.CountAsync(cancellationToken) - candidates.Count;

        var lists = candidates
            .GroupBy(c => new { c.OrderListId, c.OrderListName })
            .Select(g => new BackfillListLineDto(g.Key.OrderListId, g.Key.OrderListName, g.Count(), g.Sum(x => x.Amount)))
            .OrderBy(x => x.OrderListName)
            .ToList();

        if (request.DryRun == false)
        {
            var posting = new PaymentSafePosting(db, ledger);
            foreach (var c in candidates)
            {
                var actor = c.CreatedBy > 0 ? c.CreatedBy : currentUser.UserId ?? 0;
                var posted = await posting.PostAsync(c.Id, c.OrderId, c.OrderListId, c.Amount, actor, cancellationToken);
                if (posted.IsFailed)
                    return posted.Error!.Value;

                if (c.CreatedAt is DateTimeOffset at)
                {
                    var movement = await db.SafeMovements
                        .FirstAsync(m => m.InstallmentPaymentId == c.Id && m.Kind == SafeMovementKind.Collection, cancellationToken);
                    movement.CreatedAt = at;
                    await db.SaveChangesAsync(cancellationToken);
                }
            }

            await activityLog.AddAsync(
                request.BranchId, ActivityType.SafeDeposit, TargetEntityType.Safe, null,
                details: $"ترحيل {candidates.Count} تسديداً سابقاً إلى قاصات القوائم بمجموع {candidates.Sum(x => x.Amount):N0}");
            await db.SaveChangesAsync(cancellationToken);
        }

        return new BackfillPaymentCollectionsResponse(
            request.DryRun,
            candidates.Count,
            candidates.Sum(x => x.Amount),
            lists,
            skippedImported,
            skippedNotEntered,
            alreadyPosted);
    }
}
