namespace Application.Notifications.Queries.Admin.GetUnreadCount;

public record GetUnreadNotificationsCountQuery(int BranchId) : IRequest<Result<GetUnreadNotificationsCountResponse>>;
