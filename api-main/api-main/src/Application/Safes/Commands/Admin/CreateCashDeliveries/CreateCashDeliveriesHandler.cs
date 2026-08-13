using Application.Common.Interfaces;
using Application.Safes.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.SafeAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Commands.Admin.CreateCashDeliveries;

public class CreateCashDeliveriesHandler : IRequestHandler<CreateCashDeliveriesCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public CreateCashDeliveriesHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider,
        INotificationService notificationService, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CreateCashDeliveriesCommand request, CancellationToken cancellationToken)
    {
        var sellers = await _dbContext.Users
            .Include(x => x.OrderListAsMandob)
            .Where(x => request.SellerIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (sellers.Count != request.SellerIds.Length)
            return SafeErrors.OneOrMoreSellerNotFound;

        var safe = await _dbContext.Safes.FirstOrDefaultAsync(x => x.Id == request.SafeId, cancellationToken);
        if (safe is null)
            return SafeErrors.SafeNotFound;

        if (sellers.All(x => x.OrderListAsMandob!.BranchId == safe.BranchId) == false)
            return SafeErrors.OneOrMoreSellersDoNotBelongToThisBranch;

        foreach (var seller in sellers)
        {
            var transaction = new Transaction(seller.UndeliveredCashAmount, TransactionType.SellerPayment, TransactionStatus.Pending,
                TransactionDirection.In, request.SafeId)
            {
                SellerCashDeliveryTransaction = new SellerCashDeliveryTransaction(seller.Id, _dateTimeProvider.Today)
            };
            _dbContext.Transactions.Add(transaction);

            if (safe.BranchId is not null)
                _notificationService.AddTransactionCreatedNotification(transaction, safe.BranchId.Value);
        }

        await _activityLogService.AddAsync(
            safe.BranchId!.Value,
            ActivityType.CashDeliveriesCreated,
            TargetEntityType.Safe,
            safe.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}