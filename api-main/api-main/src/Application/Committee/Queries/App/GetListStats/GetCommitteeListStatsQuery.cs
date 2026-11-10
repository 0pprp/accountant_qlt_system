using Application.Committee.Common;
using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Committee.Queries.App.GetListStats;

public record GetCommitteeListStatsQuery(int UserId)
    : IRequest<Result<CommitteeListStatsResponse>>;

public record GetCommitteeListStatsOneQuery(int UserId, int ListId)
    : IRequest<Result<CommitteeListStatsRowDto>>;

public record CommitteeListStatsResponse
{
    public int YearMonth { get; init; }
    public List<CommitteeListStatsRowDto> Items { get; init; } = [];
}

public record CommitteeListStatsRowDto
{
    public int ListId { get; init; }
    public string? ListName { get; init; }
    public int YearMonth { get; init; }
    public int PreviousYearMonth { get; init; }
    public int NewCustomersCount { get; init; }
    public decimal NewCustomersSaleSum { get; init; }
    public decimal NewCustomersDaySalesSum { get; init; }
    public int ZeroedCustomersCount { get; init; }
    public decimal ZeroedCustomersSaleSum { get; init; }
    public decimal ZeroedCustomersDaySalesSum { get; init; }
    public int NetCustomersCount { get; init; }
    public decimal NetSaleSum { get; init; }
    public decimal NetDaySalesSum { get; init; }
    public int ActiveCustomersCount { get; init; }
    public double AverageScore { get; init; }
    public string? FinalRatingLabel { get; init; }
    public double? ScoreWeekAgo { get; init; }
    public string WeekTrend { get; init; } = "Flat";
    public double? ScoreMonthAgo { get; init; }
    public string MonthTrend { get; init; } = "Flat";
    public int ExcellentCount { get; init; }
    public int GoodCount { get; init; }
    public int WeakCount { get; init; }
    public int RejectedCount { get; init; }
    public int LegalCount { get; init; }
    public int BadCount { get; init; }
    public int RatedCustomerCount { get; init; }
    public decimal CurrentTotalSales { get; init; }
    public decimal PreviousTotalSales { get; init; }
    public decimal AddedAmount { get; init; }
    public decimal CurrentDaySales { get; init; }
    public decimal PreviousDaySales { get; init; }
    public decimal DaySalesDelta { get; init; }
    /// <summary>false حتى تتوفر لقطات شهرية مجمّدة؛ لا يُعرض صفر كقيمة محمّلة للشهر السابق إن لم تُجمّد.</summary>
    public bool PreviousMonthFrozen { get; init; }
    public bool PreviousMonthAvailable { get; init; }
}

public class GetCommitteeListStatsHandler
    : IRequestHandler<GetCommitteeListStatsQuery, Result<CommitteeListStatsResponse>>
{
    private readonly CommitteeListStatsBuilder _builder;

    public GetCommitteeListStatsHandler(CommitteeListStatsBuilder builder) => _builder = builder;

    public async Task<Result<CommitteeListStatsResponse>> Handle(
        GetCommitteeListStatsQuery request,
        CancellationToken cancellationToken)
    {
        var (yearMonth, items) = await _builder.BuildForUserAsync(request.UserId, cancellationToken);
        return new CommitteeListStatsResponse { YearMonth = yearMonth, Items = items };
    }
}

public class GetCommitteeListStatsOneHandler
    : IRequestHandler<GetCommitteeListStatsOneQuery, Result<CommitteeListStatsRowDto>>
{
    private readonly CommitteeListStatsBuilder _builder;
    private readonly IApplicationDbContext _db;

    public GetCommitteeListStatsOneHandler(CommitteeListStatsBuilder builder, IApplicationDbContext db)
    {
        _builder = builder;
        _db = db;
    }

    public async Task<Result<CommitteeListStatsRowDto>> Handle(
        GetCommitteeListStatsOneQuery request,
        CancellationToken cancellationToken)
    {
        if (!await CommitteeAccess.OwnsListAsync(_db, request.UserId, request.ListId, cancellationToken))
            return CommitteeErrors.ListNotFound;

        var row = await _builder.BuildForListAsync(request.UserId, request.ListId, cancellationToken);
        return row is null ? CommitteeErrors.ListNotFound : row;
    }
}

/// <summary>
/// معادلات ListStats المرجعية من بيانات Order الحية + لقطات تقييم الزبون.
/// لا ملخص مبيعات عام؛ PreviousMonthAvailable=false عند غياب لقطة مجمّدة (لا صفر وهمي).
/// </summary>
public class CommitteeListStatsBuilder
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public CommitteeListStatsBuilder(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<(int YearMonth, List<CommitteeListStatsRowDto> Items)> BuildForUserAsync(
        int userId,
        CancellationToken ct)
    {
        var listIds = await _db.OrderLists.AsNoTracking()
            .Where(x => x.MotabaId == userId || x.MandobId == userId)
            .Select(x => x.Id)
            .ToListAsync(ct);

        var yearMonth = ToYearMonth(BaghdadToday());
        var items = new List<CommitteeListStatsRowDto>();
        foreach (var id in listIds)
        {
            var row = await BuildForListAsync(userId, id, ct);
            if (row is not null) items.Add(row);
        }

        return (yearMonth, items);
    }

    public async Task<CommitteeListStatsRowDto?> BuildForListAsync(
        int userId,
        int listId,
        CancellationToken ct)
    {
        var list = await _db.OrderLists.AsNoTracking()
            .Where(x => x.Id == listId && (x.MotabaId == userId || x.MandobId == userId))
            .Select(x => new { x.Id, x.Name })
            .FirstOrDefaultAsync(ct);
        if (list is null) return null;

        var today = BaghdadToday();
        var yearMonth = ToYearMonth(today);
        var prevYm = PreviousYearMonth(yearMonth);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderListId == listId)
            .Select(o => new
            {
                o.Id,
                o.CustomerId,
                o.SellAmount,
                o.DailyInstallmentAmount,
                o.ExecutionStatus,
                o.SaleDate,
                Paid = o.InstallmentPayments.Sum(ip => ip.Amount),
                Created = o.CreatedAt
            })
            .ToListAsync(ct);

        var active = orders.Where(o => o.ExecutionStatus == OrderExecutionStatus.InProgress).ToList();
        var newThisMonth = orders
            .Where(o => o.SaleDate is DateOnly sd && sd >= monthStart && sd <= monthEnd)
            .ToList();
        var zeroed = orders
            .Where(o =>
                o.ExecutionStatus != OrderExecutionStatus.InProgress
                && o.SellAmount > 0
                && o.Paid >= o.SellAmount
                && o.SaleDate is DateOnly sd
                && sd >= monthStart
                && sd <= monthEnd)
            .ToList();

        var currentTotal = (decimal)active.Sum(o => o.SellAmount);
        var currentDay = (decimal)active.Sum(o => o.DailyInstallmentAmount);
        var newSale = (decimal)newThisMonth.Sum(o => o.SellAmount);
        var newDay = (decimal)newThisMonth.Sum(o => o.DailyInstallmentAmount);
        var zeroSale = (decimal)zeroed.Sum(o => o.SellAmount);
        var zeroDay = (decimal)zeroed.Sum(o => o.DailyInstallmentAmount);

        var snapDate = today;
        var ratings = await _db.FollowerCustomerRatingSnapshots.AsNoTracking()
            .Where(s => s.OrderListId == listId && s.CaptureDate == snapDate)
            .Select(s => new { s.CustomerId, s.RatingLabel, s.Score })
            .ToListAsync(ct);

        // إن لم توجد لقطة اليوم — أحدث لقطة لكل زبون خلال 30 يومًا (بدون اختراع صفر تقييم).
        if (ratings.Count == 0)
        {
            var from = today.AddDays(-30);
            var recent = await _db.FollowerCustomerRatingSnapshots.AsNoTracking()
                .Where(s => s.OrderListId == listId && s.CaptureDate >= from && s.CaptureDate <= today)
                .OrderByDescending(s => s.CaptureDate)
                .ThenByDescending(s => s.Id)
                .Select(s => new { s.CustomerId, s.RatingLabel, s.Score, s.CaptureDate })
                .ToListAsync(ct);
            ratings = recent
                .GroupBy(x => x.CustomerId)
                .Select(g => g.First())
                .Select(x => new { x.CustomerId, x.RatingLabel, x.Score })
                .ToList();
        }

        int excellent = 0, good = 0, weak = 0, rejected = 0, legal = 0, bad = 0;
        double scoreSum = 0;
        foreach (var r in ratings)
        {
            scoreSum += r.Score;
            var label = (r.RatingLabel ?? "").Trim();
            if (label.Contains("ممتاز", StringComparison.Ordinal)) excellent++;
            else if (label.Contains("جيد", StringComparison.Ordinal)) good++;
            else if (label.Contains("ضعيف", StringComparison.Ordinal)) weak++;
            else if (label.Contains("مرفوض", StringComparison.Ordinal) || label.Contains("متلكئ", StringComparison.Ordinal))
                rejected++;
            else if (label.Contains("قانون", StringComparison.Ordinal)) legal++;
            else if (label.Contains("سيء", StringComparison.Ordinal) || label.Contains("سيئ", StringComparison.Ordinal))
                bad++;
        }

        var ratedCount = ratings.Count;
        var avg = ratedCount == 0 ? 0 : Math.Round(scoreSum / ratedCount, 1);
        var finalLabel = PhantomOf(avg);

        return new CommitteeListStatsRowDto
        {
            ListId = list.Id,
            ListName = list.Name,
            YearMonth = yearMonth,
            PreviousYearMonth = prevYm,
            NewCustomersCount = newThisMonth.Select(x => x.CustomerId).Distinct().Count(),
            NewCustomersSaleSum = newSale,
            NewCustomersDaySalesSum = newDay,
            ZeroedCustomersCount = zeroed.Select(x => x.CustomerId).Distinct().Count(),
            ZeroedCustomersSaleSum = zeroSale,
            ZeroedCustomersDaySalesSum = zeroDay,
            NetCustomersCount = newThisMonth.Select(x => x.CustomerId).Distinct().Count()
                                - zeroed.Select(x => x.CustomerId).Distinct().Count(),
            NetSaleSum = newSale - zeroSale,
            NetDaySalesSum = newDay - zeroDay,
            ActiveCustomersCount = active.Select(x => x.CustomerId).Distinct().Count(),
            AverageScore = avg,
            FinalRatingLabel = ratedCount == 0 ? null : finalLabel,
            ScoreWeekAgo = null,
            WeekTrend = "Flat",
            ScoreMonthAgo = null,
            MonthTrend = "Flat",
            ExcellentCount = excellent,
            GoodCount = good,
            WeakCount = weak,
            RejectedCount = rejected,
            LegalCount = legal,
            BadCount = bad,
            RatedCustomerCount = ratedCount,
            CurrentTotalSales = currentTotal,
            PreviousTotalSales = 0,
            AddedAmount = 0,
            CurrentDaySales = currentDay,
            PreviousDaySales = 0,
            DaySalesDelta = 0,
            PreviousMonthFrozen = false,
            PreviousMonthAvailable = false
        };
    }

    private DateOnly BaghdadToday()
    {
        var utc = _clock.UtcNow;
        var baghdad = utc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(utc, DateTimeKind.Utc).AddHours(3)
            : utc.ToUniversalTime().AddHours(3);
        return DateOnly.FromDateTime(baghdad);
    }

    private static int ToYearMonth(DateOnly d) => d.Year * 100 + d.Month;

    private static int PreviousYearMonth(int yearMonth)
    {
        var y = yearMonth / 100;
        var m = yearMonth % 100;
        return m <= 1 ? (y - 1) * 100 + 12 : y * 100 + (m - 1);
    }

    private static string PhantomOf(double score) => score switch
    {
        >= 9 => "ممتاز",
        >= 7 => "جيد",
        >= 5 => "ضعيف",
        >= 3 => "متلكئ",
        _ => "سيء"
    };
}
