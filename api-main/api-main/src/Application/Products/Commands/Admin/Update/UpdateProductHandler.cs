using Application.Common.Interfaces;
using Application.Products.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Commands.Admin.Update;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public UpdateProductHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (product is null)
            return ProductErrors.ProductNotFound;

        product.Update(request.Name, request.RemainingCount, request.BuyAmount, request.SellAmount, request.DailyInstallmentAmount,
            request.Description, request.CategoryId, request.WarehouseId);

        var warehouseBranchId = await _dbContext.Warehouses
            .Where(x => x.Id == request.WarehouseId)
            .Select(x => x.BranchId)
            .FirstAsync(cancellationToken);

        await _activityLogService.AddAsync(
            warehouseBranchId,
            ActivityType.ProductUpdated,
            TargetEntityType.Product,
            request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}