using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Queries.Admin.GetPaginated;

public class GetPaginatedCustomersHandler : IRequestHandler<GetPaginatedCustomersQuery,
    Result<PaginatedList<GetPaginatedCustomersResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedCustomersHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaginatedList<GetPaginatedCustomersResponse>>> Handle(GetPaginatedCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customers.AsNoTracking()
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.FullName.Contains(request.Filter.SearchTerm!) ||
                                                              x.Business.Name.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.StartDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate)
            .Select(x => new GetPaginatedCustomersResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                BusinessName = x.Business.Name,
                BusinessAddress = x.Business.Address,
                OrderListName = x.Orders.Any() ? x.Orders.First().OrderList!.Name : null,
                OrdersCount = x.Orders.Count,
                LastInstallmentPaymentDate = x.Orders.Any() ? x.Orders
                    .SelectMany(o => o.InstallmentPayments)
                    .Max(ip => ip.Date) : null,
                CreatedAt = x.CreatedAt!.Value,
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        return customers;
    }
}