using Application.Common.Interfaces;
using Application.ProductCategories.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ProductAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.ProductCategories.Commands.Admin.Create;

public class CreateProductCategoryHandler : IRequestHandler<CreateProductCategoryCommand, Result<CreateProductCategoryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public CreateProductCategoryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }
    
    public async Task<Result<CreateProductCategoryResponse>> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var isProductCategoryExist = await _dbContext.ProductCategories.AnyAsync(x => x.Name == request.Name, cancellationToken);
        if (isProductCategoryExist)
            return ProductCategoryErrors.ProductCategoryAlreadyExist;
        
        var productCategory = new ProductCategory(request.Name);
        _dbContext.ProductCategories.Add(productCategory);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            _currentUserService.BranchIds!.First(),
            ActivityType.ProductCategoryCreated,
            TargetEntityType.ProductCategory,
            productCategory.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProductCategoryResponse(productCategory.Id);
    }
}