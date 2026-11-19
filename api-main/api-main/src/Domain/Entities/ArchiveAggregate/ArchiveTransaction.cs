using Domain.Common;
using Domain.Entities.ArchiveAggregate.Enums;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.ArchiveAggregate;

/// <summary>
/// ملفّ مبيع واحد وهو يتنقّل يداً بيد. الوحدة هي المعاملة لا الزبون، فزبون له مبيعان
/// في العشرة نفسها يظهر سطرين.
///
/// الانتقالات دوالّ <c>TryXxx</c> لا ترمي، على نمط <c>ManagementTask</c>.
/// والأختام أدناه <b>للعرض وحده</b>: الاسترجاع يُمرّ المعاملة بالمرحلة الواحدة أكثر من مرة
/// فتُكتب فوق بعضها، لذا تُحسب سرعة التدفّق من <see cref="ArchiveTransactionEvent"/> حصراً.
/// </summary>
public class ArchiveTransaction : AuditableEntity
{
    public const int ReasonMinLength = 3;
    public const int ReasonMaxLength = 500;

    private ArchiveTransaction()
    {
    }

    public ArchiveTransaction(
        int bundleId,
        int orderId,
        int customerId,
        string customerName,
        string? customerPhone,
        int? followerUserId,
        DateOnly periodSourceDate,
        ArchivePeriodSource periodSource)
    {
        BundleId = bundleId;
        OrderId = orderId;
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        FollowerUserId = followerUserId;
        PeriodSourceDate = periodSourceDate;
        PeriodSource = periodSource;
        Status = ArchiveTransactionStatus.WithSalesEmployee;
        ConcurrencyStamp = 0;
    }

    public int BundleId { get; private set; }
    public int OrderId { get; private set; }
    public int CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string? CustomerPhone { get; private set; }

    /// <summary>متابع قائمة المبيع وقت الدخول — للعرض والفرز، والاستلام بالفرع والصلاحية.</summary>
    public int? FollowerUserId { get; private set; }

    /// <summary>التاريخ الذي بُوِّبت به، محسوماً مرة واحدة عند الدخول فلا يُعيد تعديلٌ لاحق تبويبها.</summary>
    public DateOnly PeriodSourceDate { get; private set; }
    public ArchivePeriodSource PeriodSource { get; private set; }

    public ArchiveTransactionStatus Status { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    /// <summary>المبيع غادر «مُتمّ». الورق ما زال عند حامله فتبقى الحالة كما هي ليسحبه.</summary>
    public bool IsVoided { get; private set; }
    public string? VoidReason { get; private set; }
    public DateTimeOffset? VoidedAtUtc { get; private set; }

    public bool HasOpenReturn { get; private set; }
    public int ReturnCount { get; private set; }
    public string? LastReturnReason { get; private set; }
    public DateTimeOffset? LastReturnedAtUtc { get; private set; }

    public DateTimeOffset? SentToBranchAtUtc { get; private set; }
    public DateTimeOffset? ReceivedByBranchAtUtc { get; private set; }
    public int? ReceivedByBranchUserId { get; private set; }
    public DateTimeOffset? SentToFollowerAtUtc { get; private set; }
    public DateTimeOffset? ReceivedByFollowerAtUtc { get; private set; }
    public int? ReceivedByFollowerUserId { get; private set; }
    public DateTimeOffset? SentToArchiveAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }
    public int? ArchivedByUserId { get; private set; }

    public ArchiveBundle? Bundle { get; private set; }
    public Order? Order { get; private set; }
    public Customer? Customer { get; private set; }
    public User? Follower { get; private set; }
    public ICollection<ArchiveTransactionEvent> Events { get; private set; } = new HashSet<ArchiveTransactionEvent>();

    /// <summary>«منتهية» ليست <c>Archived</c> وحدها — الاسترجاع المفتوح يُبقيها قيد العمل.</summary>
    public bool IsSettled => Status == ArchiveTransactionStatus.Archived && HasOpenReturn == false;

    public bool IsInTransit => Status is ArchiveTransactionStatus.SentToBranch
        or ArchiveTransactionStatus.SentToFollower
        or ArchiveTransactionStatus.SentToArchive;

    public bool MatchesExpectedVersion(int expectedVersion) => ConcurrencyStamp == expectedVersion;

    public static ArchiveTransactionStatus? SendSourceStatus(ArchiveStage stage) => stage switch
    {
        ArchiveStage.SalesEmployee => ArchiveTransactionStatus.WithSalesEmployee,
        ArchiveStage.BranchManagement => ArchiveTransactionStatus.AtBranch,
        ArchiveStage.Follower => ArchiveTransactionStatus.AtFollower,
        _ => null
    };

    public static ArchiveTransactionStatus? ReceiveSourceStatus(ArchiveStage stage) => stage switch
    {
        ArchiveStage.BranchManagement => ArchiveTransactionStatus.SentToBranch,
        ArchiveStage.Follower => ArchiveTransactionStatus.SentToFollower,
        ArchiveStage.Archive => ArchiveTransactionStatus.SentToArchive,
        _ => null
    };

    public static ArchiveTransactionStatus? HoldingStatus(ArchiveStage stage) => stage switch
    {
        ArchiveStage.SalesEmployee => ArchiveTransactionStatus.WithSalesEmployee,
        ArchiveStage.BranchManagement => ArchiveTransactionStatus.AtBranch,
        ArchiveStage.Follower => ArchiveTransactionStatus.AtFollower,
        ArchiveStage.Archive => ArchiveTransactionStatus.Archived,
        _ => null
    };

    public bool TrySend(ArchiveStage from, DateTimeOffset atUtc, out string? error)
    {
        error = null;

        if (IsVoided)
        {
            error = "المعاملة مُبطَلة — سُحبت من المسار";
            return false;
        }

        var expected = SendSourceStatus(from);
        if (expected is null)
        {
            error = "هذه المرحلة لا ترسل";
            return false;
        }

        if (Status != expected)
        {
            error = "المعاملة ليست بعهدتك الآن";
            return false;
        }

        switch (from)
        {
            case ArchiveStage.SalesEmployee:
                Status = ArchiveTransactionStatus.SentToBranch;
                SentToBranchAtUtc = atUtc;
                break;
            case ArchiveStage.BranchManagement:
                Status = ArchiveTransactionStatus.SentToFollower;
                SentToFollowerAtUtc = atUtc;
                break;
            default:
                Status = ArchiveTransactionStatus.SentToArchive;
                SentToArchiveAtUtc = atUtc;
                break;
        }

        ConcurrencyStamp++;
        return true;
    }

    public bool TryReceive(ArchiveStage at, int actorUserId, DateTimeOffset atUtc, out string? error)
    {
        error = null;

        var expected = ReceiveSourceStatus(at);
        if (expected is null)
        {
            error = "هذه المرحلة لا تستلم";
            return false;
        }

        if (Status != expected)
        {
            error = "المعاملة ليست في انتظار استلامك";
            return false;
        }

        switch (at)
        {
            case ArchiveStage.BranchManagement:
                Status = ArchiveTransactionStatus.AtBranch;
                ReceivedByBranchAtUtc = atUtc;
                ReceivedByBranchUserId = actorUserId;
                break;
            case ArchiveStage.Follower:
                Status = ArchiveTransactionStatus.AtFollower;
                ReceivedByFollowerAtUtc = atUtc;
                ReceivedByFollowerUserId = actorUserId;
                break;
            default:
                Status = ArchiveTransactionStatus.Archived;
                ArchivedAtUtc = atUtc;
                ArchivedByUserId = actorUserId;
                HasOpenReturn = false;
                break;
        }

        ConcurrencyStamp++;
        return true;
    }

    /// <summary>
    /// تصحيح المُرسِل لفعل نفسه: «أرسلتُ بالخطأ» أو «الظرف وصل بلا الملف».
    /// ليس استرجاعاً — الاسترجاع حكمٌ على النقص ولمسؤول الأرشيف وحده.
    /// </summary>
    public bool TryUndoSend(ArchiveStage sender, DateTimeOffset atUtc, out string? error)
    {
        error = null;

        var sentStatus = sender switch
        {
            ArchiveStage.SalesEmployee => ArchiveTransactionStatus.SentToBranch,
            ArchiveStage.BranchManagement => ArchiveTransactionStatus.SentToFollower,
            ArchiveStage.Follower => ArchiveTransactionStatus.SentToArchive,
            _ => (ArchiveTransactionStatus?)null
        };

        if (sentStatus is null)
        {
            error = "هذه المرحلة لا ترسل فلا إلغاء لها";
            return false;
        }

        if (Status != sentStatus)
        {
            error = "لا يمكن إلغاء الإرسال بعد الاستلام";
            return false;
        }

        Status = HoldingStatus(sender)!.Value;
        ConcurrencyStamp++;
        return true;
    }

    /// <summary>الاسترجاع لنقص — مسؤول الأرشيف وحده، ومن <c>Archived</c> وحدها.</summary>
    public bool TryReturn(ArchiveStage destination, string reason, DateTimeOffset atUtc, out string? error)
    {
        error = null;

        if (Status != ArchiveTransactionStatus.Archived)
        {
            error = "لا تُسترجَع إلا معاملة مؤرشفة";
            return false;
        }

        if (destination == ArchiveStage.Archive)
        {
            error = "وجهة الاسترجاع يجب أن تكون مرحلة سابقة";
            return false;
        }

        var trimmed = reason?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length < ReasonMinLength)
        {
            error = "سبب الاسترجاع إجباري";
            return false;
        }

        Status = HoldingStatus(destination)!.Value;
        HasOpenReturn = true;
        ReturnCount++;
        LastReturnReason = trimmed.Length > ReasonMaxLength ? trimmed[..ReasonMaxLength] : trimmed;
        LastReturnedAtUtc = atUtc;
        ArchivedAtUtc = null;
        ArchivedByUserId = null;
        ConcurrencyStamp++;
        return true;
    }

    public bool Void(string reason, DateTimeOffset atUtc)
    {
        if (IsVoided)
            return false;

        IsVoided = true;
        VoidReason = reason;
        VoidedAtUtc = atUtc;
        ConcurrencyStamp++;
        return true;
    }

    public bool Revive()
    {
        if (IsVoided == false)
            return false;

        IsVoided = false;
        VoidReason = null;
        VoidedAtUtc = null;
        ConcurrencyStamp++;
        return true;
    }

    /// <summary>
    /// تغيّر بائع المبيع أو فرعه. يُسمح بالنقل ما دام الورق لم يتحرّك بعد؛
    /// وبعد أول إرسال يبقى مكانه لأن الورق انتقل فعلاً، ويُكتفى بصفّ حدث.
    /// </summary>
    public bool TryRebucket(int bundleId, out string? error)
    {
        error = null;

        if (Status != ArchiveTransactionStatus.WithSalesEmployee)
        {
            error = "لا تُنقل معاملة غادرت موظف المبيعات";
            return false;
        }

        if (BundleId == bundleId)
            return false;

        BundleId = bundleId;
        ConcurrencyStamp++;
        return true;
    }

    public void RefreshFollower(int? followerUserId) => FollowerUserId = followerUserId;
}
