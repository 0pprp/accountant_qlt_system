using Application.Common.Interfaces;
using Application.Products.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Commands.Admin.Delete;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public DeleteProductHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(x => x.OrderItems)
            .Include(x => x.PurchaseItems)
            .Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (product is null)
            return ProductErrors.ProductNotFound;

        if (product.OrderItems.Count != 0)
            return ProductErrors.ProductStillReferencedByOrderItems;
        
        if (product.PurchaseItems.Count != 0)
            return ProductErrors.ProductStillReferencedByPurchaseItems;

        _dbContext.Products.Remove(product);
        await _activityLogService.AddAsync(
            product.Warehouse!.BranchId,
            ActivityType.ProductDeleted,
            TargetEntityType.Product,
            request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}