using Application.Common.Interfaces;
using Application.InstallmentPayments.Common;
using Application.Orders.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Commands.Admin.Update;

public class UpdateInstallmentPaymentHandler : IRequestHandler<UpdateInstallmentPaymentCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IActivityLogService _activityLogService;

    public UpdateInstallmentPaymentHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateInstallmentPaymentCommand request, CancellationToken cancellationToken)
    {
        var installmentPayment = await _dbContext.InstallmentPayments
            .Include(x => x.Order)
            .ThenInclude(x => x!.InstallmentPayments)
            .Include(x => x.Order)
            .ThenInclude(x => x!.OrderList)
            .FirstOrDefaultAsync(
                x => x.Id == request.Id && _currentUserService.BranchIds!.Contains(x.Order!.OrderList!.BranchId), cancellationToken);
        if (installmentPayment is null)
            return InstallmentPaymentErrors.InstallmentPaymentNotFound;

        if (IsActionRestricted(installmentPayment))
            return InstallmentPaymentErrors.CanNotPerformThisAction;

        var cashHolderResult = await OrderCashHolder.GetAsync(_dbContext, installmentPayment.Order!.SellerId, cancellationToken);
        if (cashHolderResult.IsFailed)
            return cashHolderResult.Error!;

        var previousInstallmentAmount = installmentPayment.Amount;
        
        installmentPayment.Amount = request.Amount;
        installmentPayment.Description = request.Description;

        var order = installmentPayment.Order;
        var totalPaidAmount = order!.InstallmentPayments.Sum(x => x.Amount);
        if (totalPaidAmount >= order.SellAmount)
            order.ExecutionStatus = OrderExecutionStatus.Completed;

        cashHolderResult.Data!.UndeliveredCashAmount -= previousInstallmentAmount;
        cashHolderResult.Data.UndeliveredCashAmount += request.Amount;

        await _activityLogService.AddAsync(
            order!.OrderList!.BranchId,
            ActivityType.InstallmentPaymentUpdated,
            TargetEntityType.InstallmentPayment,
            request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private bool IsActionRestricted(InstallmentPayment installmentPayment)
    {
        var installmentDate = installmentPayment.Date;
        var today = _dateTimeProvider.Today;
        if (installmentDate > today)
            return true;

        if (_currentUserService.Role != Application.Common.SeedData.Roles.MainAccountant.Name)
            return false;

        var twoDaysAgo = today.AddDays(-2);

        return installmentDate < twoDaysAgo;
    }
}