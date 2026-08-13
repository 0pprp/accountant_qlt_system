using Application.Common.Interfaces;
using Application.Orders.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands.App.Delete;

public class DeleteOrderHandler : IRequestHandler<DeleteOrderCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public DeleteOrderHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(x => x.InstallmentPayments)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        if (order.SellerId != request.UserId)
            return OrderErrors.OrderDoesNotBelongToYou;

        if (order.Step == OrderStep.Completed)
            return OrderErrors.YouCanNotDeleteThisOrder;

        if (order.InstallmentPayments.Count != 0)
            return OrderErrors.YouCanNotDeleteThisOrder;

        _dbContext.Orders.Remove(order);
        await _activityLogService.AddAsync(
            order.BranchId,
            ActivityType.OrderDeleted,
            TargetEntityType.Order,
            request.OrderId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}