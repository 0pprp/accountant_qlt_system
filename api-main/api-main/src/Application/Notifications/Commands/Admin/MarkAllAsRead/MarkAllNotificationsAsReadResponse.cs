namespace Application.Notifications.Commands.Admin.MarkAllAsRead;

public record MarkAllNotificationsAsReadResponse
{
    public int UpdatedCount { get; set; }
}
