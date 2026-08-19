using Application.Common.Interfaces;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Commands.Admin.CreateCashDelivery;

public class CreateCashDeliveryHandler : IRequestHandler<CreateCashDeliveryCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public CreateCashDeliveryHandler(IApplicationDbContext dbContext, INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CreateCashDeliveryCommand request, CancellationToken cancellationToken)
    {
        var seller = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.SellerId, cancellationToken);
        if (seller is null)
            return SafeErrors.SellerNotFound;

        if (request.Amount > seller.UndeliveredCashAmount)
            return SafeErrors.RequestedAmountIsGreaterThanSellerUndeliveredCashAmount;

        var safe = await _dbContext.Safes.FirstOrDefaultAsync(x => x.Id == request.SafeId, cancellationToken);
        if (safe is null)
            return SafeErrors.SafeNotFound;

        if (safe.BranchId is null)
            return SafeErrors.SellerDoesNotBelongToThisBranch;

        var belongsToBranch = await SafeCashHolderQuery.BelongsToBranchAsync(
            _dbContext,
            seller.Id,
            safe.BranchId.Value,
            cancellationToken);
        if (belongsToBranch == false)
            return SafeErrors.SellerDoesNotBelongToThisBranch;

        var transaction = new Transaction(request.Amount, TransactionType.SellerPayment, TransactionStatus.Pending, TransactionDirection.In,
            request.SafeId)
        {
            SellerCashDeliveryTransaction = new SellerCashDeliveryTransaction(request.Description, request.Date, request.SellerId)
        };
        _dbContext.Transactions.Add(transaction);

        if (safe.BranchId is not null)
            _notificationService.AddTransactionCreatedNotification(transaction, safe.BranchId.Value);

        await _activityLogService.AddAsync(
            safe.BranchId!.Value,
            ActivityType.CashDeliveryCreated,
            TargetEntityType.Safe,
            safe.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}