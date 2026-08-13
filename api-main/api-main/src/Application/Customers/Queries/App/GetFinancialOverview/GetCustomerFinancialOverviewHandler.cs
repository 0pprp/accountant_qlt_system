using Application.Common.Interfaces;
using Application.Customers.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.App.GetFinancialOverview;

public class GetCustomerFinancialOverviewHandler : IRequestHandler<GetCustomerFinancialOverviewQuery,
    Result<GetCustomerFinancialOverviewResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetCustomerFinancialOverviewHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetCustomerFinancialOverviewResponse>> Handle(GetCustomerFinancialOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var customerExists = await _dbContext.Customers
            .AnyAsync(x => x.Id == request.CustomerId && _currentUserService.BranchIds!.Contains(x.BranchId), cancellationToken);
        if (customerExists == false)
            return CustomerErrors.CustomerNotFound;

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var aggregation = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.CustomerId == request.CustomerId)
            .Where(x => x.ApprovalStatus == OrderApprovalStatus.Approved)
            .Where(x => x.OrderList!.MandobId == request.UserId || x.OrderList.MotabaId == request.UserId)
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalPaidAmount = g.Sum(x => x.InstallmentPayments.Sum(ip => ip.Amount)),
                // Overdue Amount = |Expected - Paid|
                TotalOverdueAmount = g.Sum(x => x.ExecutionStatus == OrderExecutionStatus.Completed
                    ? 0
                    : x.DailyInstallmentAmount *
                      Math.Max(0, today.DayNumber - (x.SaleDate!.Value.DayNumber + 1)) -
                      x.InstallmentPayments
                          // Excluding the first installment (Prepayment)
                          .Where(ip => ip.Date != x.SaleDate)
                          .Where(ip => ip.Date != today)
                          .Sum(ip => ip.Amount)),
                TotalSellAmount = g.Sum(x => x.SellAmount),
                TotalDailyInstallmentAmount = g.Sum(x => x.ExecutionStatus == OrderExecutionStatus.InProgress ? x.DailyInstallmentAmount : 0),
                ActiveOrdersCount = g.Count(x => x.ExecutionStatus == OrderExecutionStatus.InProgress),
                CompletedOrdersCount = g.Count(x => x.ExecutionStatus == OrderExecutionStatus.Completed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new GetCustomerFinancialOverviewResponse
        {
            CustomerId = request.CustomerId,
            TotalPaidAmount = aggregation?.TotalPaidAmount ?? 0,
            TotalOverdueAmount = aggregation?.TotalOverdueAmount ?? 0,
            TotalSellAmount = aggregation?.TotalSellAmount ?? 0,
            TotalDailyInstallmentAmount = aggregation?.TotalDailyInstallmentAmount ?? 0,
            ActiveOrdersCount = aggregation?.ActiveOrdersCount ?? 0,
            CompletedOrdersCount = aggregation?.CompletedOrdersCount ?? 0
        };
    }
}