using Application.Common.Interfaces;
using Application.OrderLists.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.OrderListAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.OrderLists.Commands.Admin.Create;

public class CreateOrderListHandler : IRequestHandler<CreateOrderListCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public CreateOrderListHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }
    
    public async Task<Result> Handle(CreateOrderListCommand request, CancellationToken cancellationToken)
    {
        var isOrderListExist = await _dbContext.OrderLists.AnyAsync(x => x.Name == request.Name, cancellationToken);
        if (isOrderListExist)
            return OrderListErrors.OrderListAlreadyExist;
        
        var mandobHasOrderList = await _dbContext.OrderLists.AnyAsync(x => x.MandobId == request.MandobId, cancellationToken);
        if (mandobHasOrderList)
            return OrderListErrors.MandobAlreadyHasAnOrderList;

        var isMandobExist = await _dbContext.Users.AnyAsync(x => x.Id == request.MandobId, cancellationToken);
        if (isMandobExist == false)
            return OrderListErrors.MandobNotFound;
        
        var isMotabaExist = await _dbContext.Users.AnyAsync(x => x.Id == request.MotabaId, cancellationToken);
        if (isMotabaExist == false)
            return OrderListErrors.MotabaNotFound;
        
        var isBranchExist = await _dbContext.Branches.AnyAsync(x => x.Id == request.BranchId, cancellationToken);
        if (isBranchExist == false)
            return OrderListErrors.BranchNotFound;
        
        var orderList = new OrderList(request.Name, request.MandobId, request.MotabaId, request.BranchId);

        _dbContext.OrderLists.Add(orderList);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            request.BranchId,
            ActivityType.OrderListCreated,
            TargetEntityType.OrderList,
            orderList.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}