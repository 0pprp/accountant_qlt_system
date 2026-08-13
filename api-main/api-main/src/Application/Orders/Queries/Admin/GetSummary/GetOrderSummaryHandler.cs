using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.Admin.GetSummary;

public class GetOrderSummaryHandler : IRequestHandler<GetOrderSummaryQuery, Result<GetOrderSummaryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetOrderSummaryHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetOrderSummaryResponse>> Handle(GetOrderSummaryQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var order = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.Id == request.OrderId)
            .Select(x => new GetOrderSummaryResponse
            {
                Id = x.Id,
                BuyAmount = x.BuyAmount,
                SellAmount = x.SellAmount,
                PaidAmount = x.InstallmentPayments.Sum(ip => ip.Amount),
                // Overdue Amount = |Expected - Paid|
                OverdueAmount = x.ExecutionStatus == OrderExecutionStatus.Completed || x.ExecutionStatus == OrderExecutionStatus.NotStarted
                    ? 0
                    : x.DailyInstallmentAmount *
                      Math.Max(0, today.DayNumber - (x.SaleDate!.Value.DayNumber + 1)) -
                      x.InstallmentPayments
                          // Excluding the first installment (Prepayment)
                          .Where(ip => ip.Date != x.SaleDate)
                          .Where(ip => ip.Date != today)
                          .Sum(ip => ip.Amount),
                DailyInstallmentAmount = x.DailyInstallmentAmount
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        return order;
    }
}