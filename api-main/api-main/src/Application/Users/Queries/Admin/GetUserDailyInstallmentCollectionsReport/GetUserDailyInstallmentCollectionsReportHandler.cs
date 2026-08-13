using Application.Common.Interfaces;
using Application.Users.Common;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries.Admin.GetUserDailyInstallmentCollectionsReport;

public class GetUserDailyInstallmentCollectionsReportHandler : IRequestHandler<GetUserDailyInstallmentCollectionsReportQuery,
    Result<GetUserDailyInstallmentCollectionsReportResponse>>
{
    private const int DefaultLastDaysCount = 30;

    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetUserDailyInstallmentCollectionsReportHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetUserDailyInstallmentCollectionsReportResponse>> Handle(
        GetUserDailyInstallmentCollectionsReportQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.Today;
        var (startDate, endDate) = ResolveDateRange(request, today);

        var user = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == request.UserId)
            .Select(x => new { x.UndeliveredCashAmount })
            .FirstOrDefaultAsync(cancellationToken);
        if (user is null)
            return UserErrors.UserNotFound;

        var installmentsByDate = await GetDailyInstallmentsByDate(request.UserId, startDate, endDate, cancellationToken);
        var cashDeliveriesByDate = await GetDailyCashDeliveriesByDate(request.UserId, startDate, endDate, cancellationToken);
        var baselineUndelivered = await GetBaselineUndeliveredAmount(request.UserId, startDate, cancellationToken);

        var items = BuildReportItems(installmentsByDate, cashDeliveriesByDate, baselineUndelivered, startDate, endDate);

        return new GetUserDailyInstallmentCollectionsReportResponse
        {
            Items = items,
            UndeliveredCashAmount = user.UndeliveredCashAmount
        };
    }

    private static (DateOnly StartDate, DateOnly EndDate) ResolveDateRange(
        GetUserDailyInstallmentCollectionsReportQuery request, DateOnly today)
    {
        if (request.StartDate is null && request.EndDate is null)
            return (today.AddDays(-DefaultLastDaysCount), today);

        var startDate = request.StartDate ?? today.AddDays(-DefaultLastDaysCount);
        var endDate = request.EndDate ?? today;
        return (startDate, endDate);
    }

    private async Task<Dictionary<DateOnly, double>> GetDailyInstallmentsByDate(
        int userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var aggregated = await _dbContext.InstallmentPayments.AsNoTracking()
            .Where(x => x.Order!.SellerId == userId && x.Date >= startDate && x.Date <= endDate)
            .GroupBy(x => x.Date)
            .Select(x => new
            {
                Date = x.Key,
                Amount = x.Sum(i => i.Amount)
            })
            .ToListAsync(cancellationToken);

        return aggregated.ToDictionary(x => x.Date, x => x.Amount);
    }

    private async Task<Dictionary<DateOnly, double>> GetDailyCashDeliveriesByDate(
        int userId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var aggregated = await _dbContext.SellerCashDeliveryTransactions.AsNoTracking()
            .Where(x => x.SellerId == userId
                        && x.Date >= startDate
                        && x.Date <= endDate
                        && x.Transaction!.Status == TransactionStatus.Approved)
            .GroupBy(x => x.Date)
            .Select(x => new
            {
                Date = x.Key,
                Amount = x.Sum(d => d.Transaction!.Amount)
            })
            .ToListAsync(cancellationToken);

        return aggregated
            .Where(x => x.Date.HasValue)
            .ToDictionary(x => x.Date!.Value, x => x.Amount);
    }

    private async Task<double> GetBaselineUndeliveredAmount(
        int userId, DateOnly beforeDate, CancellationToken cancellationToken)
    {
        var totalInstallments = await _dbContext.InstallmentPayments.AsNoTracking()
            .Where(x => x.Order!.SellerId == userId && x.Date < beforeDate)
            .SumAsync(x => x.Amount, cancellationToken);

        var totalDeliveries = await _dbContext.SellerCashDeliveryTransactions.AsNoTracking()
            .Where(x => x.SellerId == userId
                        && x.Date < beforeDate
                        && x.Transaction!.Status == TransactionStatus.Approved)
            .SumAsync(x => x.Transaction!.Amount, cancellationToken);

        return totalInstallments - totalDeliveries;
    }

    private static List<GetUserDailyInstallmentCollectionsReportItem> BuildReportItems(
        Dictionary<DateOnly, double> installmentsByDate,
        Dictionary<DateOnly, double> cashDeliveriesByDate,
        double baselineUndelivered,
        DateOnly startDate,
        DateOnly endDate)
    {
        var cumulativeUndelivered = baselineUndelivered;

        var items = new List<GetUserDailyInstallmentCollectionsReportItem>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dailyInstallment = installmentsByDate.GetValueOrDefault(date, 0);
            var dailyDelivery = cashDeliveriesByDate.GetValueOrDefault(date, 0);

            cumulativeUndelivered += dailyInstallment - dailyDelivery;

            items.Add(new GetUserDailyInstallmentCollectionsReportItem
            {
                Date = date,
                TotalInstallmentAmount = dailyInstallment,
                TotalDeliveredCashAmount = dailyDelivery,
                CumulativeUndeliveredCashAmount = cumulativeUndelivered
            });
        }

        items.Reverse();
        return items;
    }
}