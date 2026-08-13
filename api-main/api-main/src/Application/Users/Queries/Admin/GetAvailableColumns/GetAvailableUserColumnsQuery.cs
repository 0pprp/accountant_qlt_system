namespace Application.Users.Queries.Admin.GetAvailableColumns;

public record GetAvailableUserColumnsQuery : IRequest<Result<List<GetAvailableUserColumnsResponse>>>;
