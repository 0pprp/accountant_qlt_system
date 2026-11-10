using Application.Common.Interfaces;
using Application.ManagementSummary.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

public class FollowerCollectionDataService
{
    public const int DefaultPaymentsPageSize = 50;
    public const int MaxPaymentsPageSize = 200;
    public const string CurrencyIqd = "IQD";

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly FollowerSalesReviewDataService _scope;
    private readonly IActivityLogService _activity;

    public FollowerCollectionDataService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        FollowerSalesReviewDataService scope,
        IActivityLogService activity)
    {
        _db = db;
        _clock = clock;
        _scope = scope;
        _activity = activity;
    }

    private DateOnly TodayBaghdad()
    {
        var local = TimeZoneInfo.ConvertTime(_clock.UtcNowOffset, IraqCalendar.TimeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public async Task<Result<FollowerCollectionResponse>> GetAsync(
        int followerId,
        IReadOnlyList<int> authorizedBranches,
        int? branchId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int paymentsPage,
        int paymentsPageSize,
        CancellationToken ct)
    {
        var scope = await _scope.ResolveFollowerScopeAsync(followerId, authorizedBranches, branchId, ct);
        if (scope.IsFailed)
            return scope.Error ?? FollowerWorkErrors.FollowerNotFound;

        var (_, followerName, listIds, _) = scope.Data!;
        paymentsPage = paymentsPage < 1 ? 1 : paymentsPage;
        paymentsPageSize = paymentsPageSize < 1
            ? DefaultPaymentsPageSize
            : Math.Min(paymentsPageSize, MaxPaymentsPageSize);

        var listRows = await _db.OrderLists.AsNoTracking()
            .Where(l => listIds.Contains(l.Id))
            .Select(l => new
            {
                l.Id,
                l.Name,
                l.MandobId,
                MandobName = l.Mandob != null ? l.Mandob.FullName : null,
                l.FollowerCollectionEnabled,
                l.FollowerCollectionAssignedAtUtc,
                l.CollectionToggleVersion
            })
            .OrderBy(l => l.Name)
            .ToListAsync(ct);

        var yesterday = TodayBaghdad().AddDays(-1);

        // مجاميع أمس للمندوب — تجميع DB وليس صفحة المدفوعات
        var mandobYesterday = await _db.InstallmentPayments.AsNoTracking()
            .Where(p =>
                p.Order != null
                && p.Order.OrderListId != null
                && listIds.Contains(p.Order.OrderListId.Value)
                && p.Date == yesterday
                && p.Order.OrderList != null
                && p.CreatedBy == p.Order.OrderList.MandobId)
            .GroupBy(p => p.Order!.OrderListId!.Value)
            .Select(g => new { ListId = g.Key, Amt = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.ListId, x => x.Amt, ct);

        var enableDayFollowerTotals = new Dictionary<int, (DateOnly Day, double Amt)>();
        foreach (var list in listRows.Where(l => l.FollowerCollectionAssignedAtUtc is not null))
        {
            var dayLocal = TimeZoneInfo.ConvertTime(
                list.FollowerCollectionAssignedAtUtc!.Value,
                IraqCalendar.TimeZone);
            var enableDay = DateOnly.FromDateTime(dayLocal.DateTime);
            var amt = await _db.InstallmentPayments.AsNoTracking()
                .Where(p =>
                    p.Order != null
                    && p.Order.OrderListId == list.Id
                    && p.Date == enableDay
                    && p.CreatedBy == followerId)
                .SumAsync(p => (double?)p.Amount, ct) ?? 0;
            enableDayFollowerTotals[list.Id] = (enableDay, amt);
        }

        var lists = listRows.Select(l =>
        {
            mandobYesterday.TryGetValue(l.Id, out var dAmt);
            enableDayFollowerTotals.TryGetValue(l.Id, out var fTot);
            return new FollowerCollectionListDto(
                l.Id,
                l.Name,
                l.MandobId,
                l.MandobName,
                l.FollowerCollectionEnabled,
                l.FollowerCollectionAssignedAtUtc,
                l.CollectionToggleVersion,
                yesterday,
                dAmt,
                fTot.Day == default ? null : fTot.Day,
                fTot.Amt);
        }).ToList();

        var paymentsQuery = _db.InstallmentPayments.AsNoTracking()
            .Where(p =>
                p.Order != null
                && p.Order.OrderListId != null
                && listIds.Contains(p.Order.OrderListId.Value));

        if (fromDate is DateOnly from)
            paymentsQuery = paymentsQuery.Where(p => p.Date >= from);
        if (toDate is DateOnly to)
            paymentsQuery = paymentsQuery.Where(p => p.Date <= to);

        var filteredCount = await paymentsQuery.CountAsync(ct);
        var filteredAmount = await paymentsQuery.SumAsync(p => (double?)p.Amount, ct) ?? 0;
        var totalPages = filteredCount == 0
            ? 0
            : (int)Math.Ceiling(filteredCount / (double)paymentsPageSize);

        var paymentRows = await paymentsQuery
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .Skip((paymentsPage - 1) * paymentsPageSize)
            .Take(paymentsPageSize)
            .Select(p => new
            {
                p.Id,
                p.OrderId,
                CustomerId = p.Order!.CustomerId,
                CustomerName = p.Order.Customer != null ? p.Order.Customer.FullName : null,
                ListId = p.Order.OrderListId!.Value,
                ListName = p.Order.OrderList != null ? p.Order.OrderList.Name : null,
                MandobId = p.Order.OrderList != null ? p.Order.OrderList.MandobId : 0,
                MotabaId = p.Order.OrderList != null ? p.Order.OrderList.MotabaId : 0,
                p.Amount,
                p.Date,
                p.CreatedBy
            })
            .ToListAsync(ct);

        var collectorIds = paymentRows.Select(p => p.CreatedBy).Distinct().ToList();
        var collectors = await _db.Users.AsNoTracking()
            .Where(u => collectorIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, ct);

        var payments = paymentRows.Select(p =>
        {
            collectors.TryGetValue(p.CreatedBy, out var col);
            var source = p.CreatedBy == p.MotabaId
                ? "follower"
                : p.CreatedBy == p.MandobId
                    ? "mandob"
                    : "other";
            return new FollowerCollectionPaymentDto(
                p.Id,
                p.OrderId,
                p.CustomerId,
                p.CustomerName ?? $"زبون {p.CustomerId}",
                p.ListId,
                p.ListName,
                p.Amount,
                CurrencyIqd,
                p.Date,
                p.CreatedBy == 0 ? null : p.CreatedBy,
                col?.FullName,
                source);
        }).ToList();

        var pageAmount = payments.Sum(p => p.Amount);

        return new FollowerCollectionResponse(
            lists,
            payments,
            new FollowerCollectionTotalsDto(pageAmount, filteredAmount, filteredCount, CurrencyIqd),
            filteredCount,
            paymentsPage,
            paymentsPageSize,
            totalPages,
            fromDate,
            toDate,
            FollowerWorkRules.TimeZoneId,
            _clock.UtcNowOffset,
            followerId,
            followerName);
    }

    public async Task<Result<SetFollowerCollectionEnabledResultDto>> SetEnabledAsync(
        int listId,
        bool enabled,
        int? expectedVersion,
        IReadOnlyList<int> authorizedBranches,
        int _actorUserId,
        CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var list = await _db.OrderLists
            .FirstOrDefaultAsync(l => l.Id == listId, ct);
        if (list is null)
            return FollowerWorkErrors.CollectionListOutOfScope;

        if (authorizedBranches.Contains(list.BranchId) == false)
            return FollowerWorkErrors.CollectionListOutOfScope;

        // القائمة ضمن تكليف متابع في فرع مسموح — لا نقبل followerId من العميل
        var hasAssignment = await _db.OrderLists.AsNoTracking()
            .AnyAsync(l => l.Id == listId && authorizedBranches.Contains(l.BranchId), ct);
        if (hasAssignment == false)
            return FollowerWorkErrors.CollectionListOutOfScope;

        bool changed;
        try
        {
            changed = list.SetFollowerCollectionEnabled(enabled, _clock.UtcNowOffset, expectedVersion);
        }
        catch (InvalidOperationException)
        {
            return FollowerWorkErrors.CollectionConcurrencyConflict;
        }

        if (changed)
        {
            await _activity.AddAsync(
                list.BranchId,
                ActivityType.OrderListUpdated,
                TargetEntityType.OrderList,
                list.Id);
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new SetFollowerCollectionEnabledResultDto(
            list.Id,
            list.Name,
            list.FollowerCollectionEnabled,
            list.FollowerCollectionAssignedAtUtc,
            list.CollectionToggleVersion,
            changed);
    }
}
