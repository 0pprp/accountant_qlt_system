namespace Application.OrderLists.Queries.App.GetAll;

public record GetAllOrderListsQuery(int UserId) : IRequest<Result<List<GetAllOrderListsResponse>>>;