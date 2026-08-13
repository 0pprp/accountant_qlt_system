using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Branches.Queries.Admin.GetBranchWarehouses;

public class GetBranchWarehousesHandler : IRequestHandler<GetBranchWarehousesQuery, Result<List<GetBranchWarehousesResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBranchWarehousesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<GetBranchWarehousesResponse>>> Handle(GetBranchWarehousesQuery request,
        CancellationToken cancellationToken)
    {
        var warehouses = await _dbContext.Warehouses.AsNoTracking()
            .Where(x => x.BranchId == request.BranchId)
            .Select(x => new GetBranchWarehousesResponse
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);

        return warehouses;
    }
}