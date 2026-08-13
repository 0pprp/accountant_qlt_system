namespace Application.Orders.Queries.Admin.GetSummary;

public record GetOrderSummaryQuery(int OrderId) : IRequest<Result<GetOrderSummaryResponse>>;