using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.App.GetCustomerOrders;

public class GetCustomerOrdersHandler : IRequestHandler<GetCustomerOrdersQuery, Result<List<GetCustomerOrdersResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetCustomerOrdersHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<List<GetCustomerOrdersResponse>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var orders = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.ExecutionStatus == OrderExecutionStatus.InProgress)
            .Where(x => x.CustomerId == request.CustomerId)
            .Where(x => x.OrderList!.MandobId == request.UserId || x.OrderList.MotabaId == request.UserId)
            .Select(x => new GetCustomerOrdersResponse
            {
                Id = x.Id,
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
                CreatedAt = x.CreatedAt!.Value
            })
            .ToListAsync(cancellationToken);

        return orders;
    }
}