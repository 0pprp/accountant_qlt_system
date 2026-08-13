namespace Application.Notifications.Commands.Admin.MarkAllAsRead;

public record MarkAllNotificationsAsReadCommand(int BranchId) : IRequest<Result<MarkAllNotificationsAsReadResponse>>;
