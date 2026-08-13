using Application.Branches.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.OrderLists.Queries.Admin.GetPaginated;

public class GetPaginatedOrderListsHandler : IRequestHandler<GetPaginatedOrderListsQuery,
    Result<PaginatedList<GetPaginatedOrderListsResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedOrderListsHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaginatedList<GetPaginatedOrderListsResponse>>> Handle(GetPaginatedOrderListsQuery request,
        CancellationToken cancellationToken)
    {
        var orderLists = await _dbContext.OrderLists.AsNoTracking()
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.Name.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetPaginatedOrderListsResponse
            {
                Id = x.Id,
                Name = x.Name,
                Mandob = new GetUserDto
                {
                    Id = x.MandobId,
                    FullName = x.Mandob!.FullName
                },
                Motaba = new GetUserDto
                {
                    Id = x.MotabaId,
                    FullName = x.Motaba!.FullName
                },
                Branch = new GetBranchDto
                {
                    Id = x.BranchId,
                    Name = x.Branch!.Name
                },
                CustomersCount = x.Orders.Select(o => o.CustomerId).Distinct().Count(),
                CreatedAt = x.CreatedAt!.Value
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return orderLists;
    }
}