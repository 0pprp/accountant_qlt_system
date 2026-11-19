using Application.Common.Interfaces;
using Application.InstallmentPayments.Common;
using Application.Orders.Common;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Commands.Admin.Create;

public class CreateInstallmentPaymentHandler : IRequestHandler<CreateInstallmentPaymentCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IActivityLogService _activityLogService;
    private readonly PaymentSafePosting _safePosting;

    public CreateInstallmentPaymentHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider, IActivityLogService activityLogService,
        PaymentSafePosting safePosting)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _activityLogService = activityLogService;
        _safePosting = safePosting;
    }

    public async Task<Result> Handle(CreateInstallmentPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(x => x.InstallmentPayments)
            .Include(x => x.OrderList)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.ExecutionStatus == OrderExecutionStatus.InProgress, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        if (IsCollectionActionRestricted(request.Date))
            return InstallmentPaymentErrors.CanNotPerformThisAction;
        
        var totalPaidAmount = order.InstallmentPayments.Sum(x => x.Amount);
        
        if (totalPaidAmount + request.Amount > order.SellAmount)
            return OrderErrors.PaidAmountCanNotBeGreaterThanSellAmount;

        var cashHolderResult = await OrderCashHolder.GetAsync(_dbContext, order.SellerId, cancellationToken);
        if (cashHolderResult.IsFailed)
            return cashHolderResult.Error!;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var installmentPayment = new InstallmentPayment(request.Amount, request.Date, request.Description);
        order.InstallmentPayments.Add(installmentPayment);

        if (order.InstallmentPayments.Sum(x => x.Amount) >= order.SellAmount)
            order.ExecutionStatus = OrderExecutionStatus.Completed;

        cashHolderResult.Data!.UndeliveredCashAmount += request.Amount;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var posted = order.OrderListId is int orderListId
            ? await _safePosting.PostAsync(
                installmentPayment.Id, order.Id, orderListId, installmentPayment.Amount,
                _currentUserService.UserId ?? 0, cancellationToken)
            : Result.Success();
        if (posted.IsFailed)
        {
            await transaction.RollbackAsync(cancellationToken);
            return posted.Error!;
        }

        await _activityLogService.AddAsync(
            order.OrderList.BranchId,
            ActivityType.InstallmentPaymentCreated,
            TargetEntityType.InstallmentPayment,
            installmentPayment.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }

    private bool IsCollectionActionRestricted(DateOnly paymentDate)
    {
        var today = _dateTimeProvider.Today;
        if (paymentDate > today)
            return true;

        if (_currentUserService.Role != Application.Common.SeedData.Roles.MainAccountant.Name)
            return false;

        var twoDaysAgo = today.AddDays(-2);

        return paymentDate < twoDaysAgo;
    }
}