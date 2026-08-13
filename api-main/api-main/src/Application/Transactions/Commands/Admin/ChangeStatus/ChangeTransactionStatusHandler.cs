using Application.Common.Interfaces;
using Application.Transactions.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Transactions.Commands.Admin.ChangeStatus;

public class ChangeTransactionStatusHandler : IRequestHandler<ChangeTransactionStatusCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public ChangeTransactionStatusHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(ChangeTransactionStatusCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Transactions
            .Include(x => x.Safe)
            .Include(x => x.SellerCashDeliveryTransaction)
            .ThenInclude(x => x!.Seller)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (transaction is null)
            return TransactionErrors.TransactionNotFound;

        if (request.Status == TransactionStatus.Approved) 
            transaction.Approve();

        transaction.Status = request.Status;
        transaction.StatusDescription = request.StatusDescription;

        await _activityLogService.AddAsync(
            transaction.Safe.BranchId!.Value,
            ActivityType.TransactionStatusChanged,
            TargetEntityType.Transaction,
            transaction.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}