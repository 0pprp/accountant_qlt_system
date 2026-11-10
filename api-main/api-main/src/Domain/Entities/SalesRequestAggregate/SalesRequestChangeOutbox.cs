using Domain.Common;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Domain.Entities.SalesRequestAggregate;

public class SalesRequestChangeOutbox : Entity
{
    public const int LastErrorMaxLength = 200;

    private SalesRequestChangeOutbox()
    {
    }

    public SalesRequestChangeOutbox(
        Guid eventId,
        int requestId,
        int revision,
        DateTimeOffset occurredAtUtc,
        SalesRequestChangeType changeType,
        int branchId,
        int? targetEmployeeId,
        int? previousTargetEmployeeId)
    {
        EventId = eventId;
        RequestId = requestId;
        Revision = revision;
        OccurredAtUtc = occurredAtUtc;
        ChangeType = changeType;
        BranchId = branchId;
        TargetEmployeeId = targetEmployeeId;
        PreviousTargetEmployeeId = previousTargetEmployeeId;
        RetryCount = 0;
    }

    public Guid EventId { get; private set; }
    public int RequestId { get; private set; }
    public int Revision { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public SalesRequestChangeType ChangeType { get; private set; }
    public int BranchId { get; private set; }
    public int? TargetEmployeeId { get; private set; }
    public int? PreviousTargetEmployeeId { get; private set; }
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
