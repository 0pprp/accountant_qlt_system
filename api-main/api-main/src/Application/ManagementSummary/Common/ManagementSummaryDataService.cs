using Application.Common.Interfaces;
using Application.Common.SeedData;
using Application.SalesRequests.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.ManagementSummary.Common;

public class ManagementSummaryDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public ManagementSummaryDataService(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<ManagementSummaryMatrixResponse> GetMatrixAsync(
        int year,
        int month,
        ManagementSummaryMetric metric,
        IReadOnlyList<int> branchIds,
        ICurrentUserService currentUser,
        CancellationToken ct)
    {
        var daysInMonth = IraqCalendar.DaysInMonth(year, month);
        var branches = await LoadBranchesAsync(branchIds, ct);
        var aggregates = await AggregateMetricAsync(year, month, metric, branchIds, ct);

        var cells = new List<ManagementSummaryCellDto>(daysInMonth * branches.Count);
        var dayTotals = new decimal[daysInMonth + 1];
        var branchTotals = branches.ToDictionary(b => b.Id, _ => 0m);

        foreach (var branch in branches)
        {
            for (var day = 1; day <= daysInMonth; day++)
            {
                aggregates.TryGetValue((day, branch.Id), out var amount);
                var decimalAmount = ToDecimal(amount);
                cells.Add(new ManagementSummaryCellDto(day, branch.Id, decimalAmount, "Available"));
                dayTotals[day] += decimalAmount;
                branchTotals[branch.Id] += decimalAmount;
            }
        }

        var monthTotal = branchTotals.Values.Sum();
        var boxCards = await BuildBoxCardsAsync(branchIds, currentUser, ct);

        return new ManagementSummaryMatrixResponse(
            year,
            month,
            daysInMonth,
            ManagementSummaryRules.TimeZoneId,
            ManagementSummaryRules.Currency,
            ManagementSummaryMetricParser.ToApiKey(metric),
            ManagementSummaryRules.MetricLabel(metric),
            branches,
            cells,
            Enumerable.Range(1, daysInMonth)
                .Select(d => new ManagementSummaryDayTotalDto(d, dayTotals[d]))
                .ToList(),
            branchTotals.Select(kv => new ManagementSummaryBranchTotalDto(kv.Key, kv.Value)).ToList(),
            monthTotal,
            boxCards,
            _clock.UtcNowOffset);
    }

    public async Task<Result<ManagementSummaryDetailsResponse>> GetDetailsAsync(
        int year,
        int month,
        int day,
        ManagementSummaryMetric metric,
        int branchId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var branch = await _db.Branches.AsNoTracking()
            .Where(b => b.Id == branchId)
            .Select(b => new { b.Id, b.Name })
            .FirstOrDefaultAsync(ct);
        if (branch is null)
            return ManagementSummaryErrors.BranchAccessDenied;

        var (items, totalCount, cellTotal) = await LoadDetailRowsAsync(
            year, month, day, metric, branchId, page, pageSize, ct);

        return new ManagementSummaryDetailsResponse(
            year,
            month,
            day,
            branch.Id,
            branch.Name,
            ManagementSummaryMetricParser.ToApiKey(metric),
            ManagementSummaryRules.MetricLabel(metric),
            cellTotal,
            ManagementSummaryRules.Currency,
            items,
            page,
            pageSize,
            totalCount,
            _clock.UtcNowOffset);
    }

    private async Task<IReadOnlyList<ManagementSummaryBranchDto>> LoadBranchesAsync(
        IReadOnlyList<int> branchIds,
        CancellationToken ct)
    {
        if (branchIds.Count == 0)
            return [];

        return await _db.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id))
            .OrderBy(b => b.Name)
            .Select(b => new ManagementSummaryBranchDto(b.Id, b.Name))
            .ToListAsync(ct);
    }

    private async Task<Dictionary<(int Day, int BranchId), double>> AggregateMetricAsync(
        int year,
        int month,
        ManagementSummaryMetric metric,
        IReadOnlyList<int> branchIds,
        CancellationToken ct)
    {
        if (branchIds.Count == 0)
            return new Dictionary<(int, int), double>();

        return metric switch
        {
            ManagementSummaryMetric.Receipts => await AggregateReceiptsAsync(year, month, branchIds, ct),
            ManagementSummaryMetric.Expenses => await AggregateExpensesAsync(year, month, branchIds, ct),
            ManagementSummaryMetric.SalePrice => await AggregateSalesAsync(year, month, branchIds, useBuy: false, ct),
            ManagementSummaryMetric.PurchasePrice => await AggregateSalesAsync(year, month, branchIds, useBuy: true, ct),
            ManagementSummaryMetric.Buys => await AggregateBuysAsync(year, month, branchIds, ct),
            _ => new Dictionary<(int, int), double>(),
        };
    }

    private async Task<Dictionary<(int Day, int BranchId), double>> AggregateReceiptsAsync(
        int year, int month, IReadOnlyList<int> branchIds, CancellationToken ct)
    {
        var start = new DateOnly(year, month, 1);
        var endExclusive = start.AddMonths(1);

        var rows = await _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.Date >= start
                        && p.Date < endExclusive
                        && p.Order != null
                        && p.Order.ApprovalStatus == OrderApprovalStatus.Approved
                        && p.Order.Customer != null
                        && p.Order.Customer.IsLegal == false
                        && branchIds.Contains(p.Order.BranchId))
            .GroupBy(p => new { Day = p.Date.Day, BranchId = p.Order!.BranchId })
            .Select(g => new { g.Key.Day, g.Key.BranchId, Amount = g.Sum(x => x.Amount) })
            .ToListAsync(ct);

        return rows.ToDictionary(r => (r.Day, r.BranchId), r => r.Amount);
    }

    private async Task<Dictionary<(int Day, int BranchId), double>> AggregateExpensesAsync(
        int year, int month, IReadOnlyList<int> branchIds, CancellationToken ct)
    {
        var (startUtc, endUtc) = IraqCalendar.MonthBoundsUtc(year, month);

        var rows = await _db.Expenses.AsNoTracking()
            .Where(e => e.BranchId != null
                        && branchIds.Contains(e.BranchId.Value)
                        && e.CreatedAt != null
                        && e.CreatedAt >= startUtc
                        && e.CreatedAt < endUtc)
            .Select(e => new { BranchId = e.BranchId!.Value, e.TotalAmount, e.CreatedAt })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => (IraqCalendar.ToLocalDate(r.CreatedAt!.Value).Day, r.BranchId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));
    }

    private async Task<Dictionary<(int Day, int BranchId), double>> AggregateSalesAsync(
        int year, int month, IReadOnlyList<int> branchIds, bool useBuy, CancellationToken ct)
    {
        var start = new DateOnly(year, month, 1);
        var endExclusive = start.AddMonths(1);
        var (startUtc, endUtc) = IraqCalendar.MonthBoundsUtc(year, month);

        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved
                        && branchIds.Contains(o.BranchId)
                        && ((o.SaleDate != null && o.SaleDate >= start && o.SaleDate < endExclusive)
                            || (o.SaleDate == null
                                && o.CreatedAt != null
                                && o.CreatedAt >= startUtc
                                && o.CreatedAt < endUtc)))
            .Select(o => new
            {
                o.BranchId,
                Amount = useBuy ? o.BuyAmount : o.SellAmount,
                o.SaleDate,
                o.CreatedAt
            })
            .ToListAsync(ct);

        return rows
            .Select(r =>
            {
                var day = r.SaleDate?.Day
                          ?? (r.CreatedAt is null ? 0 : IraqCalendar.ToLocalDate(r.CreatedAt.Value).Day);
                return new { Day = day, r.BranchId, r.Amount };
            })
            .Where(r => r.Day >= 1)
            .GroupBy(r => (r.Day, r.BranchId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
    }

    private async Task<Dictionary<(int Day, int BranchId), double>> AggregateBuysAsync(
        int year, int month, IReadOnlyList<int> branchIds, CancellationToken ct)
    {
        var (startUtc, endUtc) = IraqCalendar.MonthBoundsUtc(year, month);

        var rows = await _db.Purchases.AsNoTracking()
            .Where(p => p.BranchId != null
                        && branchIds.Contains(p.BranchId.Value)
                        && p.CreatedAt != null
                        && p.CreatedAt >= startUtc
                        && p.CreatedAt < endUtc)
            .Select(p => new { BranchId = p.BranchId!.Value, p.TotalAmount, p.CreatedAt })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => (IraqCalendar.ToLocalDate(r.CreatedAt!.Value).Day, r.BranchId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));
    }

    private async Task<(IReadOnlyList<ManagementSummaryDetailRowDto> Items, int TotalCount, decimal CellTotal)>
        LoadDetailRowsAsync(
            int year,
            int month,
            int day,
            ManagementSummaryMetric metric,
            int branchId,
            int page,
            int pageSize,
            CancellationToken ct)
    {
        return metric switch
        {
            ManagementSummaryMetric.Receipts => await DetailReceiptsAsync(year, month, day, branchId, page, pageSize, ct),
            ManagementSummaryMetric.Expenses => await DetailExpensesAsync(year, month, day, branchId, page, pageSize, ct),
            ManagementSummaryMetric.SalePrice => await DetailSalesAsync(year, month, day, branchId, useBuy: false, page, pageSize, ct),
            ManagementSummaryMetric.PurchasePrice => await DetailSalesAsync(year, month, day, branchId, useBuy: true, page, pageSize, ct),
            ManagementSummaryMetric.Buys => await DetailBuysAsync(year, month, day, branchId, page, pageSize, ct),
            _ => ([], 0, 0m),
        };
    }

    private async Task<(IReadOnlyList<ManagementSummaryDetailRowDto>, int, decimal)> DetailReceiptsAsync(
        int year, int month, int day, int branchId, int page, int pageSize, CancellationToken ct)
    {
        var date = new DateOnly(year, month, day);
        var query = _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.Date == date
                        && p.Order != null
                        && p.Order.BranchId == branchId
                        && p.Order.ApprovalStatus == OrderApprovalStatus.Approved
                        && p.Order.Customer != null
                        && p.Order.Customer.IsLegal == false);

        var totalCount = await query.CountAsync(ct);
        var cellTotal = ToDecimal(await query.SumAsync(p => (double?)p.Amount, ct) ?? 0);

        var rows = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Amount,
                p.Date,
                p.Description,
                OrderId = p.OrderId,
                CustomerName = p.Order!.Customer!.FullName
            })
            .ToListAsync(ct);

        var items = rows.Select(r => new ManagementSummaryDetailRowDto(
            GuidFromInt(r.Id),
            string.IsNullOrWhiteSpace(r.CustomerName) ? $"طلب #{r.OrderId}" : r.CustomerName,
            r.Description,
            ToDecimal(r.Amount),
            new DateTimeOffset(r.Date.ToDateTime(TimeOnly.MinValue), IraqCalendar.TimeZone.GetUtcOffset(r.Date.ToDateTime(TimeOnly.MinValue))),
            $"order:{r.OrderId}")).ToList();

        return (items, totalCount, cellTotal);
    }

    private async Task<(IReadOnlyList<ManagementSummaryDetailRowDto>, int, decimal)> DetailExpensesAsync(
        int year, int month, int day, int branchId, int page, int pageSize, CancellationToken ct)
    {
        var (dayStart, dayEnd) = IraqCalendar.DayBoundsUtc(new DateOnly(year, month, day));
        var query = _db.Expenses.AsNoTracking()
            .Where(e => e.BranchId == branchId
                        && e.CreatedAt != null
                        && e.CreatedAt >= dayStart
                        && e.CreatedAt < dayEnd);

        var totalCount = await query.CountAsync(ct);
        var cellTotal = ToDecimal(await query.SumAsync(e => (double?)e.TotalAmount, ct) ?? 0);

        var rows = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new { e.Id, e.FactorNumber, e.TotalAmount, e.CreatedAt })
            .ToListAsync(ct);

        var items = rows.Select(r => new ManagementSummaryDetailRowDto(
            GuidFromInt(r.Id),
            $"صرفية #{r.FactorNumber}",
            null,
            ToDecimal(r.TotalAmount),
            r.CreatedAt ?? default,
            $"expense:{r.Id}")).ToList();

        return (items, totalCount, cellTotal);
    }

    private async Task<(IReadOnlyList<ManagementSummaryDetailRowDto>, int, decimal)> DetailSalesAsync(
        int year, int month, int day, int branchId, bool useBuy, int page, int pageSize, CancellationToken ct)
    {
        var date = new DateOnly(year, month, day);
        var (dayStart, dayEnd) = IraqCalendar.DayBoundsUtc(date);

        var baseQuery = _db.Orders.AsNoTracking()
            .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved && o.BranchId == branchId)
            .Where(o => (o.SaleDate != null && o.SaleDate == date)
                        || (o.SaleDate == null
                            && o.CreatedAt != null
                            && o.CreatedAt >= dayStart
                            && o.CreatedAt < dayEnd));

        var totalCount = await baseQuery.CountAsync(ct);
        var cellTotal = ToDecimal(await baseQuery.SumAsync(
            o => (double?)(useBuy ? o.BuyAmount : o.SellAmount), ct) ?? 0);

        var rows = await baseQuery
            .OrderBy(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                Amount = useBuy ? o.BuyAmount : o.SellAmount,
                o.SaleDate,
                o.CreatedAt,
                CustomerName = o.Customer != null ? o.Customer.FullName : null
            })
            .ToListAsync(ct);

        var items = rows.Select(r =>
        {
            DateTimeOffset occurred;
            if (r.SaleDate is not null)
            {
                var local = r.SaleDate.Value.ToDateTime(TimeOnly.MinValue);
                occurred = new DateTimeOffset(local, IraqCalendar.TimeZone.GetUtcOffset(local));
            }
            else
            {
                occurred = r.CreatedAt ?? default;
            }

            return new ManagementSummaryDetailRowDto(
                GuidFromInt(r.Id),
                string.IsNullOrWhiteSpace(r.CustomerName) ? $"طلب #{r.Id}" : r.CustomerName!,
                useBuy ? "تكلفة البيع" : "سعر البيع",
                ToDecimal(r.Amount),
                occurred,
                $"order:{r.Id}");
        }).ToList();

        return (items, totalCount, cellTotal);
    }

    private async Task<(IReadOnlyList<ManagementSummaryDetailRowDto>, int, decimal)> DetailBuysAsync(
        int year, int month, int day, int branchId, int page, int pageSize, CancellationToken ct)
    {
        var (dayStart, dayEnd) = IraqCalendar.DayBoundsUtc(new DateOnly(year, month, day));
        var query = _db.Purchases.AsNoTracking()
            .Where(p => p.BranchId == branchId
                        && p.CreatedAt != null
                        && p.CreatedAt >= dayStart
                        && p.CreatedAt < dayEnd);

        var totalCount = await query.CountAsync(ct);
        var cellTotal = ToDecimal(await query.SumAsync(p => (double?)p.TotalAmount, ct) ?? 0);

        var rows = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new { p.Id, p.FactorNumber, p.TotalAmount, p.CreatedAt })
            .ToListAsync(ct);

        var items = rows.Select(r => new ManagementSummaryDetailRowDto(
            GuidFromInt(r.Id),
            $"مشترى #{r.FactorNumber}",
            null,
            ToDecimal(r.TotalAmount),
            r.CreatedAt ?? default,
            $"purchase:{r.Id}")).ToList();

        return (items, totalCount, cellTotal);
    }

    private async Task<IReadOnlyList<ManagementSummaryBoxCardDto>> BuildBoxCardsAsync(
        IReadOnlyList<int> branchIds,
        ICurrentUserService currentUser,
        CancellationToken ct)
    {
        if (currentUser.HasPermission(Application.Common.SeedData.Permissions.Safe.Read) == false)
        {
            return
            [
                new ManagementSummaryBoxCardDto(
                    "boxTotalAmountDenar",
                    "المبلغ الكلي",
                    null,
                    "Forbidden",
                    "يتطلب صلاحية Safe.Read"),
                new ManagementSummaryBoxCardDto(
                    "treasuryAmountDenar",
                    "المبلغ الفعلي",
                    null,
                    "Forbidden",
                    "يتطلب صلاحية Safe.Read"),
            ];
        }

        if (branchIds.Count == 0)
        {
            return
            [
                new ManagementSummaryBoxCardDto("boxTotalAmountDenar", "المبلغ الكلي", 0m, "Available", null),
                new ManagementSummaryBoxCardDto("treasuryAmountDenar", "المبلغ الفعلي", 0m, "Available", null),
            ];
        }

        var safes = await _db.Safes.AsNoTracking()
            .Where(s => s.BranchId != null && branchIds.Contains(s.BranchId.Value))
            .Select(s => new { BranchId = s.BranchId!.Value, s.Name, s.RemainingCashAmount })
            .ToListAsync(ct);

        var boxTotal = ToDecimal(safes.Sum(s => s.RemainingCashAmount));

        // تكييف موثّق: «الخزينة» ≈ أكبر رصيد أو اسم يحتوي خزين/قاصة لكل فرع ثم المجموع
        var treasury = 0m;
        foreach (var group in safes.GroupBy(s => s.BranchId))
        {
            var named = group.FirstOrDefault(s =>
                s.Name.Contains("خزين", StringComparison.OrdinalIgnoreCase)
                || s.Name.Contains("قاص", StringComparison.OrdinalIgnoreCase));
            treasury += ToDecimal((named ?? group.OrderByDescending(s => s.RemainingCashAmount).First()).RemainingCashAmount);
        }

        return
        [
            new ManagementSummaryBoxCardDto(
                "boxTotalAmountDenar",
                "المبلغ الكلي",
                boxTotal,
                "Available",
                "مجموع Remaining لكل صناديق الفروع المسموحة (تكييف — ليس أبو سجاد/قوائم)"),
            new ManagementSummaryBoxCardDto(
                "treasuryAmountDenar",
                "المبلغ الفعلي",
                treasury,
                "Available",
                "خزينة/قاصة الفرع أو أعلى رصيد عند غياب التسمية"),
        ];
    }

    private static decimal ToDecimal(double value) => Convert.ToDecimal(value);

    private static Guid GuidFromInt(int id)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(id).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
