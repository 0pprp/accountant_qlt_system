namespace Application.ActivityLogs.Queries.Admin.GetById;

public record GetActivityLogByIdQuery(int Id) : IRequest<Result<GetActivityLogByIdResponse>>;
