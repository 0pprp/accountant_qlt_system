using Application.Common.Interfaces;
using Application.Products.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ProductAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Commands.Admin.Create;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityLogService _activityLogService;

    public CreateProductHandler(IApplicationDbContext dbContext, IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var isCategoryExist = await _dbContext.ProductCategories.AnyAsync(x => x.Id == request.CategoryId, cancellationToken);
        if (isCategoryExist == false)
            return ProductErrors.ProductCategoryNotFound;

        var isWarehouseExist = await _dbContext.Warehouses
            .AnyAsync(x => x.Id == request.WarehouseId && x.BranchId == request.BranchId, cancellationToken);
        if (isWarehouseExist == false)
            return ProductErrors.WarehouseNotFound;

        var product = new Product(request.Name, request.RemainingCount, request.BuyAmount, request.SellAmount,
            request.DailyInstallmentAmount, request.Description, request.CategoryId)
        {
            WarehouseId = request.WarehouseId
        };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            request.BranchId,
            ActivityType.ProductCreated,
            TargetEntityType.Product,
            product.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}