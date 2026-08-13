using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands.App.SetSellerInfo;

public class SetOrderSellerInfoHandler : IRequestHandler<SetOrderSellerInfoCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INotificationService _notificationService;
    private readonly IActivityLogService _activityLogService;

    public SetOrderSellerInfoHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        INotificationService notificationService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _notificationService = notificationService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(SetOrderSellerInfoCommand request, CancellationToken cancellationToken)
    {
        var threeDaysAgo = DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddDays(-3);
        if (request.SaleDate < threeDaysAgo)
            return OrderErrors.YouCanNotAddOrderForMoreThanThreeDaysAgo;

        var order = await _dbContext.Orders
            .Include(x => x.InstallmentPayments)
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        if (order.SellerId != request.UserId)
            return OrderErrors.OrderDoesNotBelongToYou;

        if (order.Step != OrderStep.SellerInfo)
            return OrderErrors.CanNotSetSellerInfoInCurrentStep;

        var orderListIdResult = await SellerOrderListResolver.ResolveOrderListIdForSellerAsync(
            _dbContext,
            _currentUserService.Role,
            request.UserId,
            request.OrderListId,
            cancellationToken);
        if (orderListIdResult.IsFailed)
            return orderListIdResult.Error!;

        order.SetSeller(
            request.UserId,
            orderListIdResult.Data,
            request.Location,
            request.CreationAddress,
            request.SaleDate,
            request.SaleTime);

        await _notificationService.AddOrderCreatedNotificationAsync(order, cancellationToken);
        await _activityLogService.AddAsync(
            order.BranchId,
            ActivityType.OrderSellerInfoSet,
            TargetEntityType.Order,
            request.OrderId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}