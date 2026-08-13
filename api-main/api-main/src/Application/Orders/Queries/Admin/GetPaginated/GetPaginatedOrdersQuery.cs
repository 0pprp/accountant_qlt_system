using Application.Common.Models;
using Application.Common.Models.Sorting;
using Domain.Entities.OrderAggregate.Enums;

namespace Application.Orders.Queries.Admin.GetPaginated;

public record GetPaginatedOrdersQuery : IRequest<Result<GetPaginatedOrdersResponse>>
{
    public required Pagination Pagination { get; set; }
    public required GetOrdersPaginatedFilter Filter { get; set; }
    public List<string>? SelectedColumns { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetOrdersPaginatedFilter
{
    public int BranchId { get; set; }
    public string? SearchTerm { get; set; }
    public int? CustomerId { get; set; }
    public OrderStep? Step { get; set; }
    public OrderApprovalStatus? ApprovalStatus { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}