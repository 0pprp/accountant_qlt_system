using Application.Common.Interfaces;
using Application.ProductCategories.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.ProductCategories.Commands.Admin.Update;

public class UpdateProductCategoryHandler : IRequestHandler<UpdateProductCategoryCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public UpdateProductCategoryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var productCategory = await _dbContext.ProductCategories.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (productCategory is null)
            return ProductCategoryErrors.ProductCategoryNotFound;

        productCategory.Update(request.Name);
        await _activityLogService.AddAsync(
            _currentUserService.BranchIds!.First(),
            ActivityType.ProductCategoryUpdated,
            TargetEntityType.ProductCategory,
            request.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}