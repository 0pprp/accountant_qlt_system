using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.App.GetOrderInstallmentPayments;

public class GetOrderInstallmentPaymentsHandler : IRequestHandler<GetOrderInstallmentPaymentsQuery,
    Result<GetOrderInstallmentPaymentsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetOrderInstallmentPaymentsHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetOrderInstallmentPaymentsResponse>> Handle(GetOrderInstallmentPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.OrderList!.MandobId == request.UserId || x.OrderList.MotabaId == request.UserId)
            .Select(x => new
            {
                x.Id,
                CustomerFullName = x.Customer!.FullName,
                x.SellAmount,
                x.CreatedAt,
                x.SaleDate,
                x.ExecutionStatus,
                InstallmentPayments = x.InstallmentPayments.Select(ip => new
                {
                    ip.Id,
                    ip.Date,
                    ip.Amount,
                    ip.Description
                }).ToList()
            })
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order is null)
            return OrderErrors.OrderNotFound;

        var startDate = order.SaleDate!.Value;
        var endDate = order.ExecutionStatus == OrderExecutionStatus.Completed
            ? order.InstallmentPayments.Max(ip => ip.Date)
            : _dateTimeProvider.Today;

        var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
        var installmentPayments = Enumerable.Range(0, totalDays)
            .Select(dayOffset =>
            {
                var date = startDate.AddDays(dayOffset);
                var payment = order.InstallmentPayments.FirstOrDefault(ip => ip.Date == date);
                return new InstallmentPaymentDto
                {
                    Id = payment?.Id,
                    Date = date,
                    Amount = payment?.Amount,
                    Description = payment?.Description,
                    HasPayment = payment is not null
                };
            })
            .OrderByDescending(ip => ip.Date)
            .ToList();

        return new GetOrderInstallmentPaymentsResponse
        {
            Id = order.Id,
            CustomerFullName = order.CustomerFullName,
            SellAmount = order.SellAmount,
            CreatedAt = order.CreatedAt!.Value,
            InstallmentPayments = installmentPayments
        };
    }
}