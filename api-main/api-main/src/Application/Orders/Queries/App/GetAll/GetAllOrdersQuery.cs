using Domain.Entities.OrderAggregate.Enums;

namespace Application.Orders.Queries.App.GetAll;

public record GetAllOrdersQuery : IRequest<Result<List<GetAllOrdersResponse>>>
{
    public required GetPaginatedOrdersFilter Filter { get; set; }
    public int UserId { get; set; }
}

public record GetPaginatedOrdersFilter
{
    public string? FullName { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? OrderListId { get; set; }
    public OrderExecutionStatus[]? ExecutionStatuses { get; set; }
}