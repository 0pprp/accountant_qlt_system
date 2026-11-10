using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.ProvinceAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.SalesRequestAggregate;

/// <summary>
/// طلب بيع — هوية مسار العمل. Order سجل البيع المرتبط عند المسودة فقط (LinkedOrderId).
/// لا يُنشأ Order عند الإسناد.
/// </summary>
public class SalesRequest : AuditableEntity
{
    private SalesRequest()
    {
    }

    public SalesRequest(
        string customerName,
        string? customerPhone,
        SalesRequestSourceType sourceType,
        int branchId,
        int? provinceId,
        string? originalExternalId,
        int? createdByUserId)
    {
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        SourceType = sourceType;
        BranchId = branchId;
        ProvinceId = provinceId;
        OriginalExternalId = originalExternalId;
        Status = SalesRequestStatus.New;
        TargetEmployeeId = null;
        CreatedByUserId = createdByUserId;
        IsDeleted = false;
        StatusChangedAtUtc = null;
    }

    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? Address { get; private set; }
    public string? RequestedProduct { get; private set; }
    public string? SubmissionNote { get; private set; }
    public Guid? SubmissionKey { get; private set; }
    public string? SubmissionFingerprint { get; private set; }

    public void SetSubmission(string address, string product, string? note, Guid key, string fingerprint, DateTimeOffset at)
    {
        Address = address;
        RequestedProduct = product;
        SubmissionNote = note;
        SubmissionKey = key;
        SubmissionFingerprint = fingerprint;
        StatusChangedAtUtc = at;
        ConcurrencyStamp = 1;
    }

    public SalesRequestSourceType SourceType { get; set; }
    public string? OriginalExternalId { get; set; }
    public SalesRequestStatus Status { get; set; }
    public int? TargetEmployeeId { get; set; }
    public int BranchId { get; set; }
    public int? ProvinceId { get; set; }
    public int? ExistingCustomerId { get; set; }
    public int? CreatedByUserId { get; set; }
    public SalesRequestExceptionHoldStatus? ExceptionHoldStatus { get; set; }
    public string? RejectReason { get; set; }
    public string? LastStatusNote { get; set; }
    public bool IsDeleted { get; set; }
    public int ConcurrencyStamp { get; set; }

    public int? ExceptionDecidedByUserId { get; set; }
    public DateTimeOffset? ExceptionDecidedAtUtc { get; set; }
    public DateTimeOffset? ExceptionConsumedAtUtc { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }
    public string? DeleteReason { get; set; }

    /// <summary>ربط صريح بمسودة/بيع Order — فريد؛ لا تخمين بالاسم/الهاتف.</summary>
    public int? LinkedOrderId { get; set; }

    public DateTimeOffset? ViewedAtUtc { get; set; }
    public DateTimeOffset? StatusChangedAtUtc { get; set; }

    public Branch? Branch { get; set; }
    public Province? Province { get; set; }
    public Customer? ExistingCustomer { get; set; }
    public User? TargetEmployee { get; set; }
    public User? CreatedByUser { get; set; }
    public Order? LinkedOrder { get; set; }

    public bool IsUnassigned()
    {
        if (IsDeleted)
            return false;

        if (Status is SalesRequestStatus.Rejected or SalesRequestStatus.Completed or SalesRequestStatus.LegalFollowup)
            return false;

        if (HasLiveExceptionHold && TargetEmployeeId is null or <= 0)
            return false;

        return Status == SalesRequestStatus.New || TargetEmployeeId is null or <= 0;
    }

    public bool HasLiveExceptionHold
        => ExceptionHoldStatus switch
        {
            null => false,
            SalesRequestExceptionHoldStatus.Approved => ExceptionConsumedAtUtc is null,
            _ => true
        };

    public bool ConsumeApprovedException(DateTimeOffset atUtc)
    {
        if (ExceptionHoldStatus != SalesRequestExceptionHoldStatus.Approved || ExceptionConsumedAtUtc is not null)
            return false;

        ExceptionConsumedAtUtc = atUtc;
        return true;
    }

    public void ApproveException(int deciderUserId, DateTimeOffset atUtc)
        => DecideException(SalesRequestExceptionHoldStatus.Approved, deciderUserId, atUtc);

    public void RejectException(int deciderUserId, DateTimeOffset atUtc)
        => DecideException(SalesRequestExceptionHoldStatus.Rejected, deciderUserId, atUtc);

    private void DecideException(SalesRequestExceptionHoldStatus decision, int deciderUserId, DateTimeOffset atUtc)
    {
        if (IsDeleted || ExceptionHoldStatus != SalesRequestExceptionHoldStatus.Pending)
            throw new InvalidOperationException("No pending exception hold");

        ExceptionHoldStatus = decision;
        ExceptionDecidedByUserId = deciderUserId;
        ExceptionDecidedAtUtc = atUtc;
        ExceptionConsumedAtUtc = null;
        ConcurrencyStamp++;
    }

    public void SoftDelete(string reason, int deletedByUserId, DateTimeOffset atUtc)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Request already deleted");
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Delete reason required");

        IsDeleted = true;
        DeleteReason = reason.Trim();
        DeletedByUserId = deletedByUserId;
        DeletedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    public RestoreOutcome Restore(bool assigneeEligible, DateTimeOffset atUtc)
    {
        if (IsDeleted == false)
            throw new InvalidOperationException("Request is not deleted");

        var assigneeCleared = false;
        var holdDowngraded = false;

        var terminal = Status is SalesRequestStatus.Completed
            or SalesRequestStatus.Rejected
            or SalesRequestStatus.LegalFollowup;

        if (terminal == false && TargetEmployeeId is > 0 && assigneeEligible == false)
        {
            TargetEmployeeId = null;
            Status = SalesRequestStatus.New;
            ViewedAtUtc = null;
            StatusChangedAtUtc = atUtc;
            assigneeCleared = true;
        }

        if (ExceptionHoldStatus == SalesRequestExceptionHoldStatus.Approved && ExceptionConsumedAtUtc is null)
        {
            ExceptionHoldStatus = SalesRequestExceptionHoldStatus.Pending;
            ExceptionDecidedByUserId = null;
            ExceptionDecidedAtUtc = null;
            holdDowngraded = true;
        }

        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedByUserId = null;
        DeleteReason = null;
        ConcurrencyStamp++;

        return new RestoreOutcome(assigneeCleared, holdDowngraded);
    }

    public bool IsOwnedBy(int employeeId)
        => !IsDeleted && TargetEmployeeId == employeeId;

    public bool CanEmployeeTransition()
        => !IsDeleted
           && Status is not (SalesRequestStatus.Completed
               or SalesRequestStatus.Rejected
               or SalesRequestStatus.LegalFollowup);

    public void Assign(int employeeId, DateTimeOffset atUtc)
    {
        EnsureExceptionAllowsAssignment();
        ConsumeApprovedException(atUtc);
        TargetEmployeeId = employeeId;
        Status = SalesRequestStatus.Assigned;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    public void EnsureExceptionAllowsAssignment()
    {
        if (ExceptionHoldStatus is SalesRequestExceptionHoldStatus.Pending
            or SalesRequestExceptionHoldStatus.Rejected)
        {
            throw new InvalidOperationException("Exception hold blocks assignment");
        }
    }

    public void Reassign(int newEmployeeId, string? reason, DateTimeOffset atUtc)
    {
        EnsureExceptionAllowsAssignment();
        ConsumeApprovedException(atUtc);
        TargetEmployeeId = newEmployeeId;
        Status = SalesRequestStatus.Assigned;
        LastStatusNote = reason;
        StatusChangedAtUtc = atUtc;
        ViewedAtUtc = null;
        ConcurrencyStamp++;
    }

    /// <returns>true إذا تغيّرت الحالة إلى Viewed لأول مرة.</returns>
    public bool MarkViewed(DateTimeOffset atUtc)
    {
        ViewedAtUtc ??= atUtc;
        if (Status == SalesRequestStatus.Assigned)
        {
            Status = SalesRequestStatus.Viewed;
            StatusChangedAtUtc = atUtc;
            ConcurrencyStamp++;
            return true;
        }

        ConcurrencyStamp++;
        return false;
    }

    public void Prepare(string? note, DateTimeOffset atUtc)
    {
        EnsureEmployeeTransitionAllowed();
        Status = SalesRequestStatus.PreparedForSale;
        LastStatusNote = note;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    public void Pend(string note, DateTimeOffset atUtc)
    {
        EnsureEmployeeTransitionAllowed();
        if (string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("Pending note required");
        Status = SalesRequestStatus.Pending;
        LastStatusNote = note.Trim();
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    public void Reject(string? reason, DateTimeOffset atUtc, bool clearAssignee = false)
    {
        if (Status == SalesRequestStatus.Completed)
            throw new InvalidOperationException("Cannot reject completed request");
        Status = SalesRequestStatus.Rejected;
        RejectReason = reason;
        LastStatusNote = reason;
        if (clearAssignee)
            TargetEmployeeId = null;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    public void Inspect(DateTimeOffset atUtc)
    {
        EnsureEmployeeTransitionAllowed();
        Status = SalesRequestStatus.Inspected;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    public void LinkDraftOrder(int orderId, DateTimeOffset atUtc)
    {
        if (LinkedOrderId is int existing && existing != orderId)
            throw new InvalidOperationException("Request already linked to another order");
        if (LinkedOrderId == orderId)
        {
            ConcurrencyStamp++;
            return;
        }

        EnsureEmployeeTransitionAllowed();
        LinkedOrderId = orderId;
        if (Status != SalesRequestStatus.Inspected)
            Status = SalesRequestStatus.ConvertedToSale;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    public void CompleteFromSale(DateTimeOffset atUtc)
    {
        if (LinkedOrderId is null or <= 0)
            throw new InvalidOperationException("Cannot complete without linked order");
        Status = SalesRequestStatus.Completed;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    public void ReturnToEmployee(int employeeId, string note, DateTimeOffset atUtc)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("Return note required");
        TargetEmployeeId = employeeId;
        Status = SalesRequestStatus.Returned;
        LastStatusNote = note.Trim();
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    public void HoldForException(SalesRequestExceptionHoldStatus holdStatus)
    {
        ExceptionHoldStatus = holdStatus;
        ExceptionDecidedByUserId = null;
        ExceptionDecidedAtUtc = null;
        ExceptionConsumedAtUtc = null;
        TargetEmployeeId = null;
        ConcurrencyStamp++;
    }

    /// <summary>
    /// غير مسند (New بلا هدف) أو مرفوض — وفق LegalFollowupService.CanSendSalesRequestToLegal.
    /// </summary>
    public static bool CanSendToLegalFollowup(SalesRequest row)
    {
        if (row.IsDeleted)
            return false;
        if (row.HasLiveExceptionHold)
            return false;
        if (row.Status == SalesRequestStatus.Rejected)
            return true;
        return row.Status == SalesRequestStatus.New && row.TargetEmployeeId is null or <= 0;
    }

    /// <summary>إرسال للمتابعة القانونية — يصفّر تكليف موظف المبيعات.</summary>
    public void SendToLegalFollowup(string? note, DateTimeOffset atUtc)
    {
        if (CanSendToLegalFollowup(this) == false)
            throw new InvalidOperationException("يُسمح بإرسال الطلبات غير المسندة أو المرفوضة فقط للمتابعة القانونية.");

        Status = SalesRequestStatus.LegalFollowup;
        TargetEmployeeId = null;
        LastStatusNote = note;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
    }

    /// <summary>
    /// إلغاء إرسال قانونية خاطئ — يعيد الطلب لغير مسند (New) بلا حذف.
    /// </summary>
    public bool TryReturnFromLegalFollowupCancel(string reason, DateTimeOffset atUtc)
    {
        if (IsDeleted)
            return false;
        if (Status != SalesRequestStatus.LegalFollowup)
            return false;

        Status = SalesRequestStatus.New;
        TargetEmployeeId = null;
        LastStatusNote = reason;
        ViewedAtUtc = null;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
        return true;
    }

    private void EnsureEmployeeTransitionAllowed()
    {
        if (!CanEmployeeTransition())
            throw new InvalidOperationException("Illegal status transition");
    }
}
