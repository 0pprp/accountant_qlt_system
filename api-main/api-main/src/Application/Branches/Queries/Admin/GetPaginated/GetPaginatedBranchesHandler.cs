using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Branches.Queries.Admin.GetPaginated;

public class GetPaginatedBranchesHandler : IRequestHandler<GetPaginatedBranchesQuery, Result<PaginatedList<GetPaginatedBranchesResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedBranchesHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<PaginatedList<GetPaginatedBranchesResponse>>> Handle(GetPaginatedBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _dbContext.Branches.AsNoTracking()
            .When(request.Filter.SearchTerm is not null, x => x.Name.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.ProvinceId is not null, x => x.ProvinceId == request.Filter.ProvinceId)
            .Select(x => new GetPaginatedBranchesResponse
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt!.Value,
                Province = new GetProvinceDto
                {
                    Id = x.ProvinceId,
                    Name = x.Province!.Name
                }
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return branches;
    }
}