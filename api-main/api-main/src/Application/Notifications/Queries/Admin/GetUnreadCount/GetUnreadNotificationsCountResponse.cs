namespace Application.Notifications.Queries.Admin.GetUnreadCount;

public record GetUnreadNotificationsCountResponse
{
    public int UnreadCount { get; set; }
}
