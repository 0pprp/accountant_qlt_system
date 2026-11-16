using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesInventory.Queries.App.GetInventory;

public class GetSalesInventoryHandler : IRequestHandler<GetSalesInventoryQuery, Result<List<GetSalesInventoryResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetSalesInventoryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GetSalesInventoryResponse>>> Handle(GetSalesInventoryQuery request, CancellationToken cancellationToken)
    {
        var branches = _currentUserService.BranchIds ?? [];
        if (request.OrderId.HasValue || request.RequestId.HasValue)
        {
            var branch = await _dbContext.SalesRequests.AsNoTracking()
                .Where(r => !r.IsDeleted && (request.RequestId.HasValue ? r.Id == request.RequestId : r.LinkedOrderId == request.OrderId) &&
                    r.TargetEmployeeId == _currentUserService.UserId && branches.Contains(r.BranchId))
                .Select(r => (int?)r.BranchId).SingleOrDefaultAsync(cancellationToken);
            if (branch is null) return Application.SalesRequests.Common.SalesRequestErrors.NotOwner;
            branches = [branch.Value];
        }
        var products = await _dbContext.Products.AsNoTracking()
            .Where(x => branches.Contains(x.Warehouse!.BranchId))
            .OrderBy(x => x.Name)
            .Select(x => new GetSalesInventoryResponse
            {
                Id = x.Id,
                Name = x.Name,
                RemainingCount = x.RemainingCount,
                BuyAmount = x.BuyAmount,
                SellAmount = x.SellAmount,
                DailyInstallmentAmount = x.DailyInstallmentAmount,
                Description = x.Description,
                Category = new SalesInventoryCategoryDto
                {
                    Id = x.CategoryId,
                    Name = x.Category!.Name
                }
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}
