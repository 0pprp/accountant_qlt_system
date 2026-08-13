using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Commands.App.Sync;

public class SyncInstallmentPaymentsHandler : IRequestHandler<SyncInstallmentPaymentsCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public SyncInstallmentPaymentsHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(SyncInstallmentPaymentsCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstAsync(x => x.Id == request.UserId, cancellationToken);

        var groupedInstallments = request.InstallmentPayments
            .GroupBy(x => x.OrderId);

        foreach (var groupedOrder in groupedInstallments)
        {
            var orderId = groupedOrder.Key;

            var order = await _dbContext.Orders
                .Include(x => x.InstallmentPayments)
                .FirstOrDefaultAsync(x => x.Id == orderId &&
                                          (x.OrderList!.MandobId == request.UserId || x.OrderList.MotabaId == request.UserId) &&
                                          x.ExecutionStatus == OrderExecutionStatus.InProgress, cancellationToken);
            if (order is null)
                return OrderErrors.OrderNotFound;

            var installmentPayments = groupedOrder
                .Select(x => new InstallmentPayment(x.Amount, x.Date))
                .ToList();

            foreach (var installmentPayment in installmentPayments)
                order.InstallmentPayments.Add(installmentPayment);

            var totalPaidAmount = order.InstallmentPayments.Sum(x => x.Amount);
            if (totalPaidAmount >= order.SellAmount)
                order.ExecutionStatus = OrderExecutionStatus.Completed;

            user.UndeliveredCashAmount += installmentPayments.Sum(x => x.Amount);

            await _activityLogService.AddAsync(
                order.BranchId,
                ActivityType.InstallmentPaymentsSynced,
                TargetEntityType.Order,
                order.Id);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}