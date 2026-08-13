using Application.Common.Interfaces;
using Application.Safes.Common;
using Application.Transactions.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Commands.Admin.CreateSafeTransfer;

public class CreateSafeTransferHandler : IRequestHandler<CreateSafeTransferCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public CreateSafeTransferHandler(IApplicationDbContext dbContext, INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CreateSafeTransferCommand request, CancellationToken cancellationToken)
    {
        var sourceSafe = await _dbContext.Safes.FirstOrDefaultAsync(x => x.Id == request.SourceSafeId, cancellationToken);

        var destinationSafe = await _dbContext.Safes.FirstOrDefaultAsync(x => x.Id == request.DestinationSafeId, cancellationToken);
        if (destinationSafe is null)
            return SafeErrors.SafeNotFound;

        if (sourceSafe is not null && sourceSafe.BranchId.HasValue && destinationSafe.BranchId.HasValue)
            return SafeErrors.MoneyTransferBetweenTwoBranchSafeIsNotAllowed;

        if (sourceSafe is not null && request.Amount > sourceSafe.RemainingCashAmount)
            return TransactionErrors.SafeHasNotEnoughMoneyToCompleteThisTransaction;

        var transactionDirection = request.SourceSafeId is null ? TransactionDirection.In : TransactionDirection.Out;
        var safeId = request.SourceSafeId ?? request.DestinationSafeId;
        var transaction = new Transaction(request.Amount, TransactionType.SafeTransfer, TransactionStatus.Approved, transactionDirection,
            safeId)
        {
            SafeTransferTransaction = new SafeTransferTransaction(request.Description, request.SourceSafeId, request.DestinationSafeId)
        };
        _dbContext.Transactions.Add(transaction);

        if (sourceSafe?.BranchId is not null)
            _notificationService.AddTransactionCreatedNotification(transaction, sourceSafe.BranchId.Value);
        else if (destinationSafe.BranchId is not null)
            _notificationService.AddTransactionCreatedNotification(transaction, destinationSafe.BranchId.Value);

        if (sourceSafe is not null)
        {
            sourceSafe.RemainingCashAmount -= request.Amount;
            sourceSafe.CreditAmount += request.Amount;
            
            destinationSafe.RemainingCashAmount += request.Amount;
            destinationSafe.DebitAmount += request.Amount;
        }
        else
        {
            destinationSafe.RemainingCashAmount += request.Amount;
        }

        await _activityLogService.AddAsync(
            sourceSafe?.BranchId ?? destinationSafe.BranchId!.Value,
            ActivityType.SafeTransferCreated,
            TargetEntityType.Safe,
            sourceSafe?.Id ?? destinationSafe.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}