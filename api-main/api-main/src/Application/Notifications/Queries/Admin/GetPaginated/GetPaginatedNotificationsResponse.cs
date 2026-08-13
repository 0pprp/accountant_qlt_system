using Domain.Entities.NotificationAggregate.Enums;

namespace Application.Notifications.Queries.Admin.GetPaginated;

public record GetPaginatedNotificationsResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public NotificationActionType ActionType { get; set; }
    public string ActorUserFullName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool HasRead { get; set; }
}
