namespace Application.Notifications.Commands.Admin.MarkAsRead;

public record MarkNotificationAsReadCommand(int Id) : IRequest<Result>;
