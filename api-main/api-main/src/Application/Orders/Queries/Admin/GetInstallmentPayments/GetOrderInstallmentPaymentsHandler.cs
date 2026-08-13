using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.Admin.GetInstallmentPayments;

public class GetOrderInstallmentPaymentsHandler : IRequestHandler<GetOrderInstallmentPaymentsQuery,
    Result<List<GetOrderInstallmentPaymentsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;

    public GetOrderInstallmentPaymentsHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GetOrderInstallmentPaymentsResponse>>> Handle(GetOrderInstallmentPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.Id == request.OrderId && _currentUserService.BranchIds!.Contains(x.BranchId))
            .Select(x => new
            {
                x.Id,
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
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
            return OrderErrors.OrderNotFound;

        if (order.ExecutionStatus == OrderExecutionStatus.NotStarted)
            return new List<GetOrderInstallmentPaymentsResponse>();

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
                return new GetOrderInstallmentPaymentsResponse
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

        return installmentPayments;
    }
}