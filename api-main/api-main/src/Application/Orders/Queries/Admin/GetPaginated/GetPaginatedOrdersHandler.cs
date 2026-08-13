using System.Linq.Expressions;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Orders.Common;
using Domain.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.Admin.GetPaginated;

public class GetPaginatedOrdersHandler : IRequestHandler<GetPaginatedOrdersQuery, Result<GetPaginatedOrdersResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPaginatedOrdersHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetPaginatedOrdersResponse>> Handle(GetPaginatedOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Orders.AsNoTracking()
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.Customer!.FullName.Contains(request.Filter.SearchTerm!) ||
                                                              x.Seller!.FullName.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.CustomerId is not null, x => x.CustomerId == request.Filter.CustomerId)
            .When(request.Filter.Step is not null, x => x.Step == request.Filter.Step)
            .When(request.Filter.ApprovalStatus is not null, x => x.ApprovalStatus == request.Filter.ApprovalStatus)
            .When(request.Filter.StartDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var selectedFieldKeys = request.SelectedColumns?.Count > 0
            ? request.SelectedColumns
            : OrderColumns.GetAllKeys().ToList();

        var fieldSelectors = new Dictionary<string, Expression<Func<Order, object?>>>();
        foreach (var key in selectedFieldKeys)
        {
            var fieldDef = OrderColumns.GetField(key);
            if (fieldDef != null)
            {
                fieldSelectors[key] = fieldDef.Selector;
            }
        }

        var paginatedQuery = query
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt!.Value)
            .Skip((request.Pagination.PageIndex - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize);

        var items = await paginatedQuery.ProjectToDynamicAsync(fieldSelectors, cancellationToken);

        var paginatedList = new PaginatedList<Dictionary<string, object?>>(
            items,
            totalCount,
            request.Pagination.PageIndex,
            request.Pagination.PageSize);

        return new GetPaginatedOrdersResponse
        {
            PaginatedOrders = paginatedList,
            TotalBuyAmount = await query.SumAsync(x => x.BuyAmount, cancellationToken),
            TotalSellAmount = await query.SumAsync(x => x.SellAmount, cancellationToken)
        };
    }
}