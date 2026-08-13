namespace Application.Orders.Queries.Admin.GetAvailableColumns;

public record GetAvailableOrderColumnsQuery : IRequest<Result<List<GetAvailableOrderColumnsResponse>>>;

