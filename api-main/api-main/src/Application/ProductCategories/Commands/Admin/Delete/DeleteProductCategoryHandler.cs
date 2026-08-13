using Application.Common.Interfaces;
using Application.ProductCategories.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.ProductCategories.Commands.Admin.Delete;

public class DeleteProductCategoryHandler : IRequestHandler<DeleteProductCategoryCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public DeleteProductCategoryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }
    
    public async Task<Result> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var productCategory = await _dbContext.ProductCategories
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (productCategory is null)
            return ProductCategoryErrors.ProductCategoryNotFound;

        if (productCategory.Products.Count != 0)
            return ProductCategoryErrors.ProductCategoryStillReferencedByProducts;
        
        _dbContext.ProductCategories.Remove(productCategory);
        await _activityLogService.AddAsync(
            _currentUserService.BranchIds!.First(),
            ActivityType.ProductCategoryDeleted,
            TargetEntityType.ProductCategory,
            request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}