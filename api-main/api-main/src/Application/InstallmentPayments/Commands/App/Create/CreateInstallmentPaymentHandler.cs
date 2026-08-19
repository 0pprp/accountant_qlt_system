using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.InstallmentPaymentAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.InstallmentPayments.Commands.App.Create;

public class CreateInstallmentPaymentHandler : IRequestHandler<CreateInstallmentPaymentCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IActivityLogService _activityLogService;

    public CreateInstallmentPaymentHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CreateInstallmentPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(x => x.InstallmentPayments)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId &&
                                      (x.OrderList!.MandobId == request.UserId || x.OrderList.MotabaId == request.UserId) &&
                                      x.ExecutionStatus == OrderExecutionStatus.InProgress, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        var totalPaidAmount = order.InstallmentPayments.Sum(x => x.Amount);

        if (totalPaidAmount + request.Amount > order.SellAmount)
            return OrderErrors.PaidAmountCanNotBeGreaterThanSellAmount;

        var cashHolderResult = await OrderCashHolder.GetAsync(_dbContext, order.SellerId, cancellationToken);
        if (cashHolderResult.IsFailed)
            return cashHolderResult.Error!;

        var installmentPayment = new InstallmentPayment(request.Amount, _dateTimeProvider.Today);
        order.InstallmentPayments.Add(installmentPayment);

        if (order.InstallmentPayments.Sum(x => x.Amount) == order.SellAmount)
            order.ExecutionStatus = OrderExecutionStatus.Completed;

        cashHolderResult.Data!.UndeliveredCashAmount += request.Amount;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            order.BranchId,
            ActivityType.InstallmentPaymentCreated,
            TargetEntityType.InstallmentPayment,
            installmentPayment.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}