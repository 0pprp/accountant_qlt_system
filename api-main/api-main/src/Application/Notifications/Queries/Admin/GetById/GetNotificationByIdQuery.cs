namespace Application.Notifications.Queries.Admin.GetById;

public record GetNotificationByIdQuery(int Id) : IRequest<Result<GetNotificationByIdResponse>>;
