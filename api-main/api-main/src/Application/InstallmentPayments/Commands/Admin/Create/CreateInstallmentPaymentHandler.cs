using Application.Common.Interfaces;
using Application.InstallmentPayments.Common;
using Application.Orders.Common;
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

    public CreateInstallmentPaymentHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CreateInstallmentPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(x => x.InstallmentPayments)
            .Include(x => x.OrderList)
            .ThenInclude(x => x!.Mandob)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && x.ExecutionStatus == OrderExecutionStatus.InProgress, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        if (IsCollectionActionRestricted(request.Date))
            return InstallmentPaymentErrors.CanNotPerformThisAction;
        
        var totalPaidAmount = order.InstallmentPayments.Sum(x => x.Amount);
        
        if (totalPaidAmount + request.Amount > order.SellAmount)
            return OrderErrors.PaidAmountCanNotBeGreaterThanSellAmount;

        var installmentPayment = new InstallmentPayment(request.Amount, request.Date, request.Description);
        order.InstallmentPayments.Add(installmentPayment);

        if (order.InstallmentPayments.Sum(x => x.Amount) >= order.SellAmount)
            order.ExecutionStatus = OrderExecutionStatus.Completed;

        order.OrderList!.Mandob!.UndeliveredCashAmount += request.Amount;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            order.OrderList.BranchId,
            ActivityType.InstallmentPaymentCreated,
            TargetEntityType.InstallmentPayment,
            installmentPayment.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

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