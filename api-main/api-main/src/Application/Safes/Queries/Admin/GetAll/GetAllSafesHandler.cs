using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Safes.Queries.Admin.GetAll;

public class GetAllSafesHandler : IRequestHandler<GetAllSafesQuery, Result<List<GetAllSafesResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllSafesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<List<GetAllSafesResponse>>> Handle(GetAllSafesQuery request, CancellationToken cancellationToken)
    {
        var safes = await _dbContext.Safes.AsNoTracking()
            .Select(x => new GetAllSafesResponse
            {
                Id = x.Id,
                Name = x.Name,
                BranchId = x.BranchId
            })
            .ToListAsync(cancellationToken);

        return safes;
    }
}