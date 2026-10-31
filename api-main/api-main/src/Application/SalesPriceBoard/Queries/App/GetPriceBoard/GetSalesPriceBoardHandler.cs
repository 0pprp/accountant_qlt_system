using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesPriceBoard.Queries.App.GetPriceBoard;

public class GetSalesPriceBoardHandler : IRequestHandler<GetSalesPriceBoardQuery, Result<List<GetSalesPriceBoardResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetSalesPriceBoardHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GetSalesPriceBoardResponse>>> Handle(GetSalesPriceBoardQuery request, CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products.AsNoTracking()
            .Where(x => _currentUserService.BranchIds!.Contains(x.Warehouse!.BranchId))
            .Where(x => x.RemainingCount > 0)
            .OrderBy(x => x.Category!.Name)
            .ThenBy(x => x.Name)
            .Select(x => new GetSalesPriceBoardResponse
            {
                Id = x.Id,
                Name = x.Name,
                SellAmount = x.SellAmount,
                DailyInstallmentAmount = x.DailyInstallmentAmount,
                RemainingCount = x.RemainingCount,
                CategoryName = x.Category!.Name
            })
            .ToListAsync(cancellationToken);

        return products;
    }
}