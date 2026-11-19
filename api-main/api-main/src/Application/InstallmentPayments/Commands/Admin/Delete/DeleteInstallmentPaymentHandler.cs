using Application.Common.Interfaces;
using Application.InstallmentPayments.Common;
using Application.Orders.Common;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Commands.Admin.Delete;

public class DeleteInstallmentPaymentHandler : IRequestHandler<DeleteInstallmentPaymentCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;
    private readonly PaymentSafePosting _safePosting;
    private readonly ICurrentUserService _currentUserService;

    public DeleteInstallmentPaymentHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService,
        PaymentSafePosting safePosting, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
        _safePosting = safePosting;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteInstallmentPaymentCommand request, CancellationToken cancellationToken)
    {
        var installmentPayment = await _dbContext.InstallmentPayments
            .Include(x => x.Order)
            .ThenInclude(x => x!.OrderList)
            .Include(x => x.Order)
            .ThenInclude(x => x!.InstallmentPayments)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (installmentPayment is null)
            return InstallmentPaymentErrors.InstallmentPaymentNotFound;
        
        var order = installmentPayment.Order;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var reversed = await _safePosting.AdjustAsync(
            installmentPayment.Id, 0, _currentUserService.UserId, cancellationToken);
        if (reversed.IsFailed)
        {
            await transaction.RollbackAsync(cancellationToken);
            return reversed.Error!;
        }

        var cashHolderResult = await OrderCashHolder.GetAsync(_dbContext, order!.SellerId, cancellationToken);
        if (cashHolderResult.IsFailed)
            return cashHolderResult.Error!;

        cashHolderResult.Data!.UndeliveredCashAmount -= installmentPayment.Amount;
        
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
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}