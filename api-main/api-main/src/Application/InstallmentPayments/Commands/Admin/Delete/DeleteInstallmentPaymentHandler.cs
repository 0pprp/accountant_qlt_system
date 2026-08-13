using Application.Common.Interfaces;
using Application.InstallmentPayments.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Commands.Admin.Delete;

public class DeleteInstallmentPaymentHandler : IRequestHandler<DeleteInstallmentPaymentCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public DeleteInstallmentPaymentHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(DeleteInstallmentPaymentCommand request, CancellationToken cancellationToken)
    {
        var installmentPayment = await _dbContext.InstallmentPayments
            .Include(x => x.Order)
            .ThenInclude(x => x!.OrderList)
            .ThenInclude(x => x!.Mandob)
            .Include(x => x.Order)
            .ThenInclude(x => x!.InstallmentPayments)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (installmentPayment is null)
            return InstallmentPaymentErrors.InstallmentPaymentNotFound;
        
        var order = installmentPayment.Order;

        order!.OrderList!.Mandob!.UndeliveredCashAmount -= installmentPayment.Amount;
        
        var totalPaidAmount = order.InstallmentPayments.Sum(x => x.Amount) - installmentPayment.Amount;
        if (totalPaidAmount < order.SellAmount)
            order.ExecutionStatus = OrderExecutionStatus.InProgress;

        _dbContext.InstallmentPayments.Remove(installmentPayment);
        await _activityLogService.AddAsync(
            order!.OrderList!.BranchId,
            ActivityType.InstallmentPaymentDeleted,
            TargetEntityType.InstallmentPayment,
            request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}