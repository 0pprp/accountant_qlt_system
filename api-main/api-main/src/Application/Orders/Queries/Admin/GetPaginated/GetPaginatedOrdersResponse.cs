using Application.Common.Models;

namespace Application.Orders.Queries.Admin.GetPaginated;

public record GetPaginatedOrdersResponse
{
    public required PaginatedList<Dictionary<string, object?>> PaginatedOrders { get; set; }
    public double TotalBuyAmount { get; set; }
    public double TotalSellAmount { get; set; }
}