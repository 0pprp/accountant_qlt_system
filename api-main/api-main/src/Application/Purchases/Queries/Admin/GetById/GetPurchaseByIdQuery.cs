namespace Application.Purchases.Queries.Admin.GetById;

public record GetPurchaseByIdQuery(int Id) : IRequest<Result<GetPurchaseByIdResponse>>;