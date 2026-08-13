namespace Application.Users.Queries.Admin.GetById;

public record GetUserByIdQuery(int Id) : IRequest<Result<GetUserByIdResponse>>;