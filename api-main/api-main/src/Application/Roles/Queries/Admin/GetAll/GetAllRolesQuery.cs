namespace Application.Roles.Queries.Admin.GetAll;

public record GetAllRolesQuery : IRequest<Result<List<GetAllRolesResponse>>>;