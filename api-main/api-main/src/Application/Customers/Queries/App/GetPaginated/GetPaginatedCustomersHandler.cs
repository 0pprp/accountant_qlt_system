using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.App.GetPaginated;

public class GetPaginatedCustomersHandler : IRequestHandler<GetPaginatedCustomersQuery,
    Result<List<GetPaginatedCustomersResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPaginatedCustomersHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GetPaginatedCustomersResponse>>> Handle(GetPaginatedCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customers.AsNoTracking()
            .Where(x => _currentUserService.BranchIds!.Contains(x.BranchId))
            .When(request.Filter.SearchTerm is not null, x => x.FullName.Contains(request.Filter.SearchTerm!) ||
                                                              x.MotherName.Contains(request.Filter.SearchTerm!) ||
                                                              x.Business.Name.Contains(request.Filter.SearchTerm!))
            .Select(x => new GetPaginatedCustomersResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                MotherName = x.MotherName,
                BusinessName = x.Business.Name,
                CreatedAt = x.CreatedAt!.Value,
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .KeysetPaginateAsync(key: x => x.CreatedAt, pagination: request.Pagination, cancellationToken: cancellationToken);

        return customers;
    }
}