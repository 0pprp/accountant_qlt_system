namespace Application.Users.Queries.Admin.GetTabAccess;

public sealed record GetUserTabAccessQuery(int UserId) : IRequest<Result<GetUserTabAccessResponse>>;
