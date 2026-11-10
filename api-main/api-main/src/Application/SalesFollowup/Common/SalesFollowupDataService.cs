using Application.Common.Interfaces;
using Application.SalesRatings.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesFollowup.Common;

/// <summary>
/// تجميع بيانات متابعة المبيعات دفعة واحدة (بدون N+1 من المتصفح).
/// </summary>
public class SalesFollowupDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public SalesFollowupDataService(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    /// <summary>
    /// كروت موظفي المبيعات: تكليف SalesEmployeeAssignment نشط في النطاق.
    /// التقييم من Order.SellerId (إسناد البيع الأصلي) — ليس نقل مبيعات عند إعادة التكليف.
    /// </summary>
    public async Task<IReadOnlyList<SalesFollowupEmployeeCardDto>> ListEmployeesAsync(
        IReadOnlyList<int> branchIds,
        CancellationToken ct)
    {
        if (branchIds.Count == 0)
            return [];

        var asOf = _clock.Today;
        var now = _clock.UtcNowOffset;

        var employeeIds = await Application.SalesEmployees.Common.SalesWorkforce
            .ActiveUserIdsAsync(_db, now, branchIds, ct);
        if (employeeIds.Count == 0)
            return [];

        var orderRows = await _db.Orders.AsNoTracking()
            .Where(o => branchIds.Contains(o.BranchId)
                        && o.SellerId != null
                        && employeeIds.Contains(o.SellerId.Value)
                        && o.ApprovalStatus == OrderApprovalStatus.Approved)
            .Select(o => new
            {
                SellerId = o.SellerId!.Value,
                o.BranchId,
                o.CustomerId,
                o.SellAmount,
                o.SaleDate,
                Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
                LastPaymentDate = o.InstallmentPayments.Max(ip => (DateOnly?)ip.Date),
            })
            .ToListAsync(ct);

        var users = await _db.Users.AsNoTracking()
            .Where(u => employeeIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(ct);

        var assignmentBranches = await Application.SalesEmployees.Common.SalesWorkforce
            .ActiveAssignments(_db, now, branchIds)
            .Select(a => new { a.UserId, a.BranchId })
            .ToListAsync(ct);

        var branchNames = await _db.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id))
            .Select(b => new { b.Id, b.Name })
            .ToDictionaryAsync(b => b.Id, b => b.Name, ct);

        var managerRatings = await LatestManagerRatingsAsync(employeeIds, ct);
        var cards = new List<SalesFollowupEmployeeCardDto>(employeeIds.Count);

        foreach (var empId in employeeIds)
        {
            var empOrders = orderRows.Where(o => o.SellerId == empId).ToList();
            var customers = empOrders
                .GroupBy(o => o.CustomerId)
                .Select(g =>
                {
                    var total = g.Sum(x => x.SellAmount);
                    var receipts = g.Sum(x => x.Receipts);
                    var remaining = total - receipts;
                    var saleDate = g.Min(x => x.SaleDate);
                    var lastPay = g.Max(x => x.LastPaymentDate);
                    return CustomerRatingCalculator.ScoreFor(total, receipts, remaining, saleDate, lastPay, asOf);
                })
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();

            var avg = customers.Count == 0 ? 0 : customers.Average();
            managerRatings.TryGetValue(empId, out var mgr);
            var user = users.FirstOrDefault(u => u.Id == empId);

            var assignedBranchIds = assignmentBranches.Where(a => a.UserId == empId).Select(a => a.BranchId).ToList();
            var primaryBranchId = empOrders.Count > 0
                ? empOrders.GroupBy(o => o.BranchId)
                    .OrderByDescending(g => g.Sum(x => x.SellAmount))
                    .Select(g => g.Key)
                    .FirstOrDefault()
                : assignedBranchIds.FirstOrDefault();
            var branchLabel = string.Join(" · ",
                assignedBranchIds.Select(id => branchNames.GetValueOrDefault(id) ?? $"#{id}"));

            cards.Add(new SalesFollowupEmployeeCardDto
            {
                EmployeeId = empId,
                EmployeeName = user?.FullName ?? $"#{empId}",
                BranchId = primaryBranchId,
                BranchName = string.IsNullOrWhiteSpace(branchLabel) ? "—" : branchLabel,
                CustomerCount = empOrders.Select(o => o.CustomerId).Distinct().Count(),
                AverageScore = Math.Round(avg, 2),
                RatingLabel = SalesFollowupRules.AverageRatingLabel(avg),
                ManagerScore = mgr?.Score,
                ManagerNote = mgr?.Note,
                ManagerRatedAt = mgr?.RatedAtUtc,
            });
        }

        return cards.OrderBy(c => c.EmployeeName).ToList();
    }

    public async Task<SalesFollowupDetailDto?> GetDetailAsync(
        int employeeId,
        int branchId,
        CancellationToken ct)
    {
        var inBranch = await _db.Orders.AsNoTracking()
            .AnyAsync(o => o.SellerId == employeeId
                           && o.BranchId == branchId
                           && o.ApprovalStatus == OrderApprovalStatus.Approved, ct);

        if (!inBranch)
            return null;

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == employeeId)
            .Select(u => new { u.Id, u.FullName })
            .FirstOrDefaultAsync(ct);
        if (user is null)
            return null;

        var branchName = await _db.Branches.AsNoTracking()
            .Where(b => b.Id == branchId)
            .Select(b => b.Name)
            .FirstOrDefaultAsync(ct);

        var asOf = _clock.Today;
        var facts = await LoadCustomerFactsAsync(employeeId, branchId, asOf, ct);
        var scored = facts.Where(f => f.Score.HasValue).ToList();
        var avg = scored.Count == 0 ? 0 : scored.Average(f => f.Score!.Value);

        var mgr = await _db.DirectorSalesEmployeeBoardRatings.AsNoTracking()
            .Where(x => x.EmployeeUserId == employeeId)
            .OrderByDescending(x => x.RatedAtUtc)
            .FirstOrDefaultAsync(ct);

        var lowRows = scored
            .Where(f => SalesFollowupRules.IsLow(f.Score!.Value))
            .Select(f => new SalesFollowupRatingRowDto
            {
                Score = f.Score!.Value,
                Note = null,
                RatingDate = (f.DateSaleDevice ?? asOf).ToDateTime(TimeOnly.MinValue),
                CustomerName = f.CustomerName,
                ListName = f.ListName,
                Kind = "system",
            })
            .ToList();

        var anonymizedLow = SalesFollowupRules.AnonymizeLowOnly(lowRows);

        var history = BuildSystemAverageHistory(facts, asOf);
        var classCounts = BuildClassCounts(facts);
        var byList = BuildSalesByList(facts);
        var conversion = BuildConversion(facts, asOf);

        var blocked = await _db.SalesEmployeeAppBlocks.AsNoTracking()
            .Where(x => x.EmployeeUserId == employeeId)
            .Select(x => x.IsBlocked)
            .FirstOrDefaultAsync(ct);

        return new SalesFollowupDetailDto
        {
            Employee = new SalesFollowupEmployeeCardDto
            {
                EmployeeId = employeeId,
                EmployeeName = user.FullName,
                BranchId = branchId,
                BranchName = branchName,
                CustomerCount = facts.Count,
                AverageScore = Math.Round(avg, 2),
                RatingLabel = SalesFollowupRules.AverageRatingLabel(avg),
                ManagerScore = mgr?.Score,
                ManagerNote = mgr?.Note,
                ManagerRatedAt = mgr?.RatedAtUtc,
            },
            ManagerRating = mgr is null
                ? null
                : new SalesFollowupManagerRatingDto
                {
                    Score = mgr.Score,
                    Note = mgr.Note,
                    RatedAt = mgr.RatedAtUtc,
                    RatedByUserName = mgr.RatedByUserName,
                },
            FollowerSaleAuthRatings = new SalesFollowupRatingBucketDto { Low = anonymizedLow.ToList() },
            CollectionDelegateRatings = new SalesFollowupRatingBucketDto(),
            SystemRatingHistory = history.ToList(),
            ZeroedCustomerCount = SalesFollowupRules.CountZeroedCustomers(facts),
            Customers = facts,
            SalesByList = byList,
            ClassCounts = classCounts,
            Conversion = conversion,
            IsAppBlocked = blocked,
            Marker = SalesFollowupRules.DeployMarker,
        };
    }

    private async Task<List<SalesFollowupCustomerFact>> LoadCustomerFactsAsync(
        int sellerUserId,
        int branchId,
        DateOnly asOf,
        CancellationToken ct)
    {
        var orderRows = await _db.Orders.AsNoTracking()
            .Where(x => x.SellerId == sellerUserId
                        && x.BranchId == branchId
                        && x.ApprovalStatus == OrderApprovalStatus.Approved)
            .Select(x => new
            {
                x.CustomerId,
                CustomerName = x.Customer!.FullName,
                Phone = x.Customer.PhoneNumber,
                Address = x.Customer.Business.Address,
                ShopName = x.Customer.Business.Name,
                OrderListId = x.OrderListId,
                ListName = x.OrderList != null ? x.OrderList.Name : null,
                x.SellAmount,
                SaleDate = x.SaleDate,
                Payments = x.InstallmentPayments.Select(ip => new { ip.Amount, ip.Date }).ToList(),
                ItemNames = x.OrderItems.Select(i => i.ProductName).ToList(),
            })
            .ToListAsync(ct);

        var merged = orderRows
            .GroupBy(r => r.CustomerId)
            .Select(g =>
            {
                var first = g.First();
                var totalSales = g.Sum(x => x.SellAmount);
                var payments = g.SelectMany(x => x.Payments).ToList();
                var receipts = payments.Sum(p => p.Amount);
                var remaining = totalSales - receipts;
                var saleDate = g.Min(x => x.SaleDate);
                var lastPay = payments.Count == 0 ? (DateOnly?)null : payments.Max(p => p.Date);
                var lastPayAmount = payments.Count == 0
                    ? 0
                    : payments.OrderByDescending(p => p.Date).First().Amount;
                var score = CustomerRatingCalculator.ScoreFor(
                    totalSales, receipts, remaining, saleDate, lastPay, asOf);
                var days = saleDate is null
                    ? 0
                    : CustomerRatingCalculator.DiffDaysInclusive(
                        saleDate.Value,
                        lastPay ?? asOf);
                var rate = totalSales <= 0 || days <= 0
                    ? 0
                    : receipts * 1000.0 / (totalSales * days);

                return new SalesFollowupCustomerFact
                {
                    CustomerId = first.CustomerId,
                    CustomerName = first.CustomerName,
                    PhoneNumber = first.Phone,
                    Address = first.Address,
                    ShopName = first.ShopName,
                    AmountTotalSales = totalSales,
                    ReceiptsTotal = receipts,
                    AmountRemaining = remaining,
                    AmountDaySales = 0,
                    DateSaleDevice = saleDate,
                    LastPaymentDate = lastPay,
                    LastPaymentAmount = lastPayAmount,
                    CountReceiptDevice = payments.Count,
                    NumberOfDayDevice = days,
                    ReceiptRateDevice = Math.Round(rate, 2),
                    Score = score,
                    RatingLabel = score is null ? "—" : CustomerRatingCalculator.LevelName(score.Value),
                    IsLegal = false,
                    ItemsNames = string.Join("، ", g.SelectMany(x => x.ItemNames).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct()),
                    OrderListId = first.OrderListId,
                    ListName = first.ListName,
                };
            })
            .OrderBy(f => f.CustomerName)
            .ToList();

        return merged;
    }

    private async Task<Dictionary<int, Domain.Entities.SalesFollowupAggregate.DirectorSalesEmployeeBoardRating>> LatestManagerRatingsAsync(
        IReadOnlyList<int> employeeIds,
        CancellationToken ct)
    {
        if (employeeIds.Count == 0)
            return new Dictionary<int, Domain.Entities.SalesFollowupAggregate.DirectorSalesEmployeeBoardRating>();

        var all = await _db.DirectorSalesEmployeeBoardRatings.AsNoTracking()
            .Where(x => employeeIds.Contains(x.EmployeeUserId))
            .ToListAsync(ct);

        return all
            .GroupBy(x => x.EmployeeUserId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.RatedAtUtc).First());
    }

    private static IReadOnlyList<SalesFollowupSystemDayAvgDto> BuildSystemAverageHistory(
        IReadOnlyList<SalesFollowupCustomerFact> facts,
        DateOnly asOf)
    {
        var result = new List<SalesFollowupSystemDayAvgDto>(SalesFollowupRules.SystemAverageHistoryDays);
        for (var i = 0; i < SalesFollowupRules.SystemAverageHistoryDays; i++)
        {
            var day = asOf.AddDays(-i);
            var dayScores = new List<int>();
            foreach (var f in facts)
            {
                if (f.DateSaleDevice is null || f.AmountTotalSales <= 0)
                    continue;
                var score = CustomerRatingCalculator.ScoreFor(
                    f.AmountTotalSales,
                    f.ReceiptsTotal,
                    f.AmountRemaining,
                    f.DateSaleDevice,
                    f.LastPaymentDate.HasValue && f.LastPaymentDate.Value <= day ? f.LastPaymentDate : null,
                    day);
                if (score is not null)
                    dayScores.Add(score.Value);
            }

            var avg = dayScores.Count == 0 ? 0 : dayScores.Average();
            result.Add(new SalesFollowupSystemDayAvgDto
            {
                RatingDate = day.ToDateTime(TimeOnly.MinValue),
                AverageScore = Math.Round(avg, 2),
                RatingLabel = SalesFollowupRules.AverageRatingLabel(avg),
                CustomerCount = dayScores.Count,
            });
        }

        return result;
    }

    private static List<SalesFollowupClassCountDto> BuildClassCounts(IReadOnlyList<SalesFollowupCustomerFact> facts)
    {
        var labels = new[] { "ممتاز", "جيد", "ضعيف", "متلكئ", "سيء" };
        var counts = labels.Select(l => new SalesFollowupClassCountDto
        {
            Label = l,
            Count = facts.Count(f => f.RatingLabel == l && f.AmountRemaining > 0),
        }).ToList();
        counts.Add(new SalesFollowupClassCountDto
        {
            Label = "قانونية",
            Count = facts.Count(f => f.IsLegal),
        });
        counts.Add(new SalesFollowupClassCountDto
        {
            Label = "المصفرين",
            Count = SalesFollowupRules.CountZeroedCustomers(facts),
        });
        return counts;
    }

    private static List<SalesFollowupSalesByListDto> BuildSalesByList(IReadOnlyList<SalesFollowupCustomerFact> facts) =>
        facts
            .GroupBy(f => new { f.OrderListId, Name = f.ListName ?? "بدون قائمة" })
            .Select(g => new SalesFollowupSalesByListDto
            {
                ListId = g.Key.OrderListId,
                ListName = g.Key.Name,
                CustomerCount = g.Count(),
                TotalSales = g.Sum(x => x.AmountTotalSales),
                TotalRemaining = g.Sum(x => x.AmountRemaining),
            })
            .OrderByDescending(x => x.TotalSales)
            .ToList();

    private static readonly string[] ConversionTargets = ["ممتاز", "جيد", "ضعيف", "متلكئ", "سيء", "قانونية"];

    private static SalesFollowupConversionDto BuildConversion(
        IReadOnlyList<SalesFollowupCustomerFact> facts,
        DateOnly asOf)
    {
        var open = facts.Where(f => f.AmountRemaining > 0).ToList();
        var improved = 0;
        var declined = 0;
        var unchanged = 0;

        string LabelAt(SalesFollowupCustomerFact f, DateOnly day)
        {
            if (f.IsLegal) return "قانونية";
            var score = CustomerRatingCalculator.ScoreFor(
                f.AmountTotalSales, f.ReceiptsTotal, f.AmountRemaining,
                f.DateSaleDevice,
                f.LastPaymentDate.HasValue && f.LastPaymentDate.Value <= day ? f.LastPaymentDate : null,
                day);
            return score is null ? "—" : CustomerRatingCalculator.LevelName(score.Value);
        }

        var rank = new Dictionary<string, int>
        {
            ["سيء"] = 0, ["متلكئ"] = 1, ["ضعيف"] = 2, ["جيد"] = 3, ["ممتاز"] = 4, ["قانونية"] = 5,
        };

        foreach (var f in open)
        {
            var today = LabelAt(f, asOf);
            var yesterday = LabelAt(f, asOf.AddDays(-1));
            if (!rank.ContainsKey(today) || !rank.ContainsKey(yesterday))
            {
                unchanged++;
                continue;
            }

            var delta = rank[today] - rank[yesterday];
            if (delta > 0) improved++;
            else if (delta < 0) declined++;
            else unchanged++;
        }

        var targets = new List<SalesFollowupConversionTargetDto>();
        foreach (var target in ConversionTargets)
        {
            var arrivedToday = open
                .Where(f => LabelAt(f, asOf) == target && LabelAt(f, asOf.AddDays(-1)) != target)
                .ToList();

            var fromBreakdown = arrivedToday
                .GroupBy(f => LabelAt(f, asOf.AddDays(-1)))
                .Select(g => new SalesFollowupConversionFromDto { FromLabel = g.Key, Count = g.Count() })
                .ToList();

            var last30 = new List<SalesFollowupConversionTargetDayDto>();
            for (var i = 0; i < 30; i++)
            {
                var day = asOf.AddDays(-i);
                var prev = day.AddDays(-1);
                var dayArrivals = open
                    .Where(f => LabelAt(f, day) == target && LabelAt(f, prev) != target)
                    .ToList();
                last30.Add(new SalesFollowupConversionTargetDayDto
                {
                    Date = day,
                    ArrivedCount = dayArrivals.Count,
                    FromBreakdown = dayArrivals
                        .GroupBy(f => LabelAt(f, prev))
                        .Select(g => new SalesFollowupConversionFromDto { FromLabel = g.Key, Count = g.Count() })
                        .ToList(),
                    Customers = dayArrivals.Select(f => new SalesFollowupConversionCustomerDto
                    {
                        CustomerId = f.CustomerId,
                        CustomerName = f.CustomerName,
                        FromLabel = LabelAt(f, prev),
                    }).ToList(),
                });
            }

            targets.Add(new SalesFollowupConversionTargetDto
            {
                TargetLabel = target,
                ArrivedCount = arrivedToday.Count,
                DetailText = string.Join(" · ", fromBreakdown.Select(x => $"{x.FromLabel}:{x.Count}")),
                FromBreakdown = fromBreakdown,
                Customers = arrivedToday.Select(f => new SalesFollowupConversionCustomerDto
                {
                    CustomerId = f.CustomerId,
                    CustomerName = f.CustomerName,
                    FromLabel = LabelAt(f, asOf.AddDays(-1)),
                }).ToList(),
                Last30Days = last30,
            });
        }

        return new SalesFollowupConversionDto
        {
            Improved = improved,
            Declined = declined,
            Unchanged = unchanged,
            SummaryText = $"ازداد {improved} · نقص {declined} · مساوي {unchanged}",
            Targets = targets,
        };
    }
}
