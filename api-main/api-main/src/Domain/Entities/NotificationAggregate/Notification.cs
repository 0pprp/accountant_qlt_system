using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.NotificationAggregate.Enums;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.NotificationAggregate;

public class Notification : AuditableEntity
{
    public Notification(
        string title,
        string description,
        NotificationActionType actionType,
        int branchId,
        int actorUserId)
    {
        Title = title;
        Description = description;
        ActionType = actionType;
        BranchId = branchId;
        ActorUserId = actorUserId;
        HasRead = false;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public NotificationActionType ActionType { get; set; }
    public bool HasRead { get; set; }
    public int BranchId { get; set; }
    public int ActorUserId { get; set; }

    public Branch? Branch { get; set; }
    public User? ActorUser { get; set; }

    public void MarkAsRead()
    {
        HasRead = true;
    }
}