using Domain.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Domain.Entities.ManagementTaskAggregate;

public class ManagementTaskChangeOutbox : Entity
{
    public const int LastErrorMaxLength = 200;

    private ManagementTaskChangeOutbox()
    {
    }

    public ManagementTaskChangeOutbox(
        Guid eventId,
        int taskId,
        int revision,
        DateTimeOffset occurredAtUtc,
        ManagementTaskChangeType changeType,
        int branchId,
        int currentHolderUserId,
        int assigneeUserId)
    {
        EventId = eventId;
        TaskId = taskId;
        Revision = revision;
        OccurredAtUtc = occurredAtUtc;
        ChangeType = changeType;
        BranchId = branchId;
        CurrentHolderUserId = currentHolderUserId;
        AssigneeUserId = assigneeUserId;
        RetryCount = 0;
    }

    public Guid EventId { get; private set; }
    public int TaskId { get; private set; }
    public int Revision { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public ManagementTaskChangeType ChangeType { get; private set; }
    public int BranchId { get; private set; }
    public int CurrentHolderUserId { get; private set; }
    public int AssigneeUserId { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset? LastAttemptAtUtc { get; private set; }
    public string? LastError { get; private set; }

    public void MarkPublished(DateTimeOffset atUtc)
    {
        PublishedAtUtc = atUtc;
        LastAttemptAtUtc = atUtc;
        LastError = null;
    }

    public void MarkFailed(DateTimeOffset atUtc, string? error)
    {
        RetryCount++;
        LastAttemptAtUtc = atUtc;
        LastError = error is { Length: > LastErrorMaxLength } ? error[..LastErrorMaxLength] : error;
    }
}
