using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Provinces.Queries.Admin.GetPaginated;

public class GetPaginatedProvincesHandler : IRequestHandler<GetPaginatedProvincesQuery, Result<PaginatedList<GetPaginatedProvincesResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedProvincesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<PaginatedList<GetPaginatedProvincesResponse>>> Handle(GetPaginatedProvincesQuery request, CancellationToken cancellationToken)
    {
        var provinces = await _dbContext.Provinces.AsNoTracking()
            .Select(x => new GetPaginatedProvincesResponse
            {
                Id = x.Id,
                Name = x.Name
            })
            .Sort(request.SortCriteria)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return provinces;
    }
}