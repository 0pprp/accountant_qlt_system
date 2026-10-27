using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Customers.Common;
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
        var asOf = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

        var page = await _dbContext.Customers.AsNoTracking()
            .Where(x => x.BranchId == request.Filter.BranchId)
            .When(request.Filter.SearchTerm is not null, x => x.FullName.Contains(request.Filter.SearchTerm!) ||
                                                              x.Business.Name.Contains(request.Filter.SearchTerm!))
            .When(request.Filter.StartDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) >= request.Filter.StartDate)
            .When(request.Filter.EndDate is not null, x => DateOnly.FromDateTime(x.CreatedAt!.Value.DateTime) <= request.Filter.EndDate)
            .Select(x => new CustomerListRatingRow
            {
                Id = x.Id,
                FullName = x.FullName,
                BusinessName = x.Business.Name,
                BusinessAddress = x.Business.Address,
                OrderListName = x.Orders.Any() ? x.Orders.First().OrderList!.Name : null,
                OrdersCount = x.Orders.Count,
                LastInstallmentPaymentDate = x.Orders.SelectMany(o => o.InstallmentPayments).Any()
                    ? x.Orders.SelectMany(o => o.InstallmentPayments).Max(ip => ip.Date)
                    : null,
                CreatedAt = x.CreatedAt!.Value,
                SellAmount = x.Orders.Sum(o => o.SellAmount),
                PaidAmount = x.Orders.SelectMany(o => o.InstallmentPayments).Sum(ip => ip.Amount),
                SaleDate = x.Orders
                    .Where(o => o.SaleDate != null)
                    .Select(o => o.SaleDate)
                    .Min(),
                EarliestOrderCreated = x.Orders
                    .Where(o => o.CreatedAt != null)
                    .Select(o => (DateTimeOffset?)o.CreatedAt)
                    .Min(),
            })
            .SortOrDefault(request.SortCriteria, x => x.CreatedAt)
            .ToPaginatedListAsync(request.Pagination, cancellationToken);

        var items = page.Items.Select(row =>
        {
            var saleDate = row.SaleDate
                           ?? (row.EarliestOrderCreated is { } created
                               ? DateOnly.FromDateTime(created.DateTime)
                               : (DateOnly?)null);
            var remaining = row.SellAmount - row.PaidAmount;
            return new GetPaginatedCustomersResponse
            {
                Id = row.Id,
                FullName = row.FullName,
                BusinessName = row.BusinessName,
                BusinessAddress = row.BusinessAddress,
                OrderListName = row.OrderListName,
                OrdersCount = row.OrdersCount,
                LastInstallmentPaymentDate = row.LastInstallmentPaymentDate,
                CreatedAt = row.CreatedAt,
                Rating = CustomerRatingCalculator.EvaluateLabel(
                    row.SellAmount,
                    row.PaidAmount,
                    remaining,
                    saleDate,
                    row.LastInstallmentPaymentDate,
                    asOf),
            };
        }).ToList();

        return new PaginatedList<GetPaginatedCustomersResponse>(
            items,
            page.TotalCount,
            page.PageIndex,
            page.PageSize);
    }

    private sealed class CustomerListRatingRow
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string BusinessName { get; set; }
        public required string BusinessAddress { get; set; }
        public string? OrderListName { get; set; }
        public int OrdersCount { get; set; }
        public DateOnly? LastInstallmentPaymentDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public double SellAmount { get; set; }
        public double PaidAmount { get; set; }
        public DateOnly? SaleDate { get; set; }
        public DateTimeOffset? EarliestOrderCreated { get; set; }
    }
}
