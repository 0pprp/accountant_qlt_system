using Domain.Common;
using Domain.Entities.ArchiveAggregate.Enums;

namespace Domain.Entities.ArchiveAggregate;

/// <summary>
/// صندوق بريد معاملاتي: يُكتب في نفس حفظ تغيير الحالة فيلتزم معه أو يسقط معه،
/// ثم تلتقطه خدمة خلفية وتبثّه. نسخة من <c>ManagementTaskChangeOutbox</c>.
/// </summary>
public class ArchiveChangeOutbox : Entity
{
    public const int LastErrorMaxLength = 200;

    private ArchiveChangeOutbox()
    {
    }

    public ArchiveChangeOutbox(
        Guid eventId,
        int bundleId,
        int? transactionId,
        DateTimeOffset occurredAtUtc,
        ArchiveEventType changeType,
        ArchiveStage? targetStage,
        int branchId,
        int salesEmployeeUserId,
        int? followerUserId)
    {
        EventId = eventId;
        BundleId = bundleId;
        TransactionId = transactionId;
        OccurredAtUtc = occurredAtUtc;
        ChangeType = changeType;
        TargetStage = targetStage;
        BranchId = branchId;
        SalesEmployeeUserId = salesEmployeeUserId;
        FollowerUserId = followerUserId;
        RetryCount = 0;
    }

    public Guid EventId { get; private set; }
    public int BundleId { get; private set; }
    public int? TransactionId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public ArchiveEventType ChangeType { get; private set; }

    /// <summary>المرحلة التي يعنيها التغيير — من يُشعَر به.</summary>
    public ArchiveStage? TargetStage { get; private set; }

    public int BranchId { get; private set; }
    public int SalesEmployeeUserId { get; private set; }
    public int? FollowerUserId { get; private set; }

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
