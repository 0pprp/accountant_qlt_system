using Application.Common.Interfaces;
using Application.OrderLists.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.OrderLists.Commands.Admin.Update;

public class UpdateOrderListHandler : IRequestHandler<UpdateOrderListCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public UpdateOrderListHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateOrderListCommand request, CancellationToken cancellationToken)
    {
        var isOrderListExist = await _dbContext.OrderLists.AnyAsync(x => x.Name == request.Name && x.Id != request.Id, cancellationToken);
        if (isOrderListExist)
            return OrderListErrors.OrderListAlreadyExist;

        var orderList = await _dbContext.OrderLists.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (orderList is null)
            return OrderListErrors.OrderListNotFound;

        var mandobHasOrderList = await _dbContext.OrderLists.AnyAsync(x => x.Id != orderList.Id && x.MandobId == request.MandobId,
            cancellationToken);
        if (mandobHasOrderList)
            return OrderListErrors.MandobAlreadyHasAnOrderList;

        orderList.Update(request.Name, request.MandobId, request.MotabaId, request.BranchId);
        await _activityLogService.AddAsync(
            request.BranchId,
            ActivityType.OrderListUpdated,
            TargetEntityType.OrderList,
            request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}