using Application.Common.Interfaces;
using Application.OrderLists.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.OrderLists.Queries.App.GetAll;

public class GetAllOrderListsHandler : IRequestHandler<GetAllOrderListsQuery, Result<List<GetAllOrderListsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetAllOrderListsHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<List<GetAllOrderListsResponse>>> Handle(GetAllOrderListsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var orderLists = await _dbContext.OrderLists.AsNoTracking()
            .Where(x => x.MandobId == request.UserId || x.MotabaId == request.UserId)
            .Select(x => new GetAllOrderListsResponse
            {
                Id = x.Id,
                Name = x.Name,
                MandobFullName = x.Mandob!.FullName,
                MandobPhoneNumber = x.Mandob.PhoneNumber,
                TotalOrderCount = x.Orders.Count(o => o.ExecutionStatus == OrderExecutionStatus.InProgress),
                TodayCollectedOrderCount = x.Orders.Count(o => o.ExecutionStatus == OrderExecutionStatus.InProgress &&
                                                               o.InstallmentPayments.Any(ip => ip.Date == _dateTimeProvider.Today)),
                TotalSellAmount = x.Orders
                    .Where(o => o.ExecutionStatus == OrderExecutionStatus.InProgress)
                    .Sum(o => o.SellAmount),
                TotalDailyInstallmentAmount = x.Orders
                    .Where(o => o.ExecutionStatus == OrderExecutionStatus.InProgress)
                    .Sum(o => o.DailyInstallmentAmount),

                // Overdue Amount For each approved order = |Expected - Paid|
                TotalOverdueInstallmentAmount = x.Orders
                    .Where(o => o.ExecutionStatus == OrderExecutionStatus.InProgress)
                    .Select(o => new
                    {
                        o.DailyInstallmentAmount,
                        SaleDate = o.SaleDate!.Value,
                        o.InstallmentPayments
                    })
                    .Select(o => new
                    {
                        RequiredPaymentDays = today.DayNumber - (o.SaleDate.DayNumber + 1),
                        o.DailyInstallmentAmount,
                        // Excluding the first installment (Prepayment) and today installment
                        PaidAmount = o.InstallmentPayments
                            .Where(ip => ip.Date != o.SaleDate)
                            .Where(ip => ip.Date != today)
                            .Sum(ip => ip.Amount)
                    })
                    .Select(o => o.DailyInstallmentAmount * Math.Max(0, o.RequiredPaymentDays) - o.PaidAmount)
                    .Sum(),
                TotalCollectedInstallmentAmount = x.Orders
                    .Where(o => o.ExecutionStatus == OrderExecutionStatus.InProgress)
                    .SelectMany(o => o.InstallmentPayments
                        .Select(ip => ip.Amount))
                    .Sum(),
                TotalUnpaidInstallmentAmount = x.Orders
                    .Where(o => o.ExecutionStatus == OrderExecutionStatus.InProgress)
                    .Select(o => new
                    {
                        CollectedAmount = o.InstallmentPayments
                            .Select(ip => ip.Amount)
                            .Sum(),
                        o.SellAmount
                    })
                    .Select(o => o.SellAmount - o.CollectedAmount)
                    .Sum()
            })
            .ToListAsync(cancellationToken);
        if (orderLists.Count == 0)
            return OrderListErrors.YouHaveNotOrderListYet;

        return orderLists;
    }
}