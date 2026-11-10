using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Domain.Entities.OrderAggregate;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.ManagementTaskAggregate;

public class ManagementTask : AuditableEntity
{
    private ManagementTask()
    {
    }

    public ManagementTask(
        string title,
        string description,
        ManagementTaskPriority priority,
        DateOnly dueDate,
        DateTimeOffset? dueAtUtc,
        int branchId,
        int createdByUserId,
        int assigneeUserId,
        string assigneeDisplayName,
        int? customerId,
        int? orderId)
    {
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        DueAtUtc = dueAtUtc;
        BranchId = branchId;
        CreatedByUserId = createdByUserId;
        AssigneeUserId = assigneeUserId;
        AssigneeDisplayName = assigneeDisplayName;
        CurrentHolderUserId = assigneeUserId;
        CurrentHolderDisplayName = assigneeDisplayName;
        CustomerId = customerId;
        OrderId = orderId;
        Status = ManagementTaskStatus.Sent;
        ConcurrencyStamp = 0;
    }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ManagementTaskPriority Priority { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateTimeOffset? DueAtUtc { get; private set; }
    public ManagementTaskStatus Status { get; private set; }
    public int BranchId { get; private set; }
    public int CreatedByUserId { get; private set; }
    public int AssigneeUserId { get; private set; }
    public string AssigneeDisplayName { get; private set; } = string.Empty;
    public int CurrentHolderUserId { get; private set; }
    public string CurrentHolderDisplayName { get; private set; } = string.Empty;
    public string? RejectNote { get; private set; }
    public int? CustomerId { get; private set; }
    public int? OrderId { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public DateTimeOffset? TransferredAtUtc { get; private set; }
    public DateTimeOffset? BranchCompletedAtUtc { get; private set; }
    public DateTimeOffset? AssigneeCompletedAtUtc { get; private set; }
    public DateTimeOffset? ConfirmedAtUtc { get; private set; }
    public DateTimeOffset? StatusChangedAtUtc { get; private set; }

    public Branch? Branch { get; private set; }
    public User? CreatedByUser { get; private set; }
    public User? AssigneeUser { get; private set; }
    public User? CurrentHolderUser { get; private set; }
    public Customer? Customer { get; private set; }
    public Order? LinkedOrder { get; private set; }

    public ICollection<ManagementTaskEvent> Events { get; private set; } = new List<ManagementTaskEvent>();
    public ICollection<ManagementTaskAttachment> Attachments { get; private set; } = new List<ManagementTaskAttachment>();

    public bool HasActiveTransfer => TransferredAtUtc.HasValue;

    public bool IsCurrentHolder(int userId) => CurrentHolderUserId == userId;

    public bool MatchesExpectedVersion(int expectedVersion) => ConcurrencyStamp == expectedVersion;

    public bool TryAccept(int actorUserId, DateTimeOffset atUtc, out string? error)
    {
        error = null;
        if (IsCurrentHolder(actorUserId) == false)
        {
            error = "لست حامل المهمة الحالي.";
            return false;
        }

        if (Status is not (ManagementTaskStatus.Sent or ManagementTaskStatus.Transferred))
        {
            error = "لا يمكن قبول المهمة في حالتها الحالية.";
            return false;
        }

        Status = ManagementTaskStatus.Accepted;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
        return true;
    }

    public bool TryReject(int actorUserId, string note, DateTimeOffset atUtc, out string? error)
    {
        error = null;
        if (IsCurrentHolder(actorUserId) == false)
        {
            error = "لست حامل المهمة الحالي.";
            return false;
        }

        if (Status is not (ManagementTaskStatus.Sent or ManagementTaskStatus.Transferred))
        {
            error = "لا يمكن رفض المهمة في حالتها الحالية.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(note))
        {
            error = "ملاحظة الرفض مطلوبة.";
            return false;
        }

        RejectNote = note.Trim();
        Status = ManagementTaskStatus.Returned;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
        return true;
    }

    public bool TryResend(
        int newAssigneeUserId,
        string newAssigneeDisplayName,
        string title,
        string description,
        ManagementTaskPriority priority,
        DateOnly dueDate,
        DateTimeOffset? dueAtUtc,
        int? customerId,
        int? orderId,
        DateTimeOffset atUtc,
        out string? error)
    {
        error = null;
        if (Status != ManagementTaskStatus.Returned)
        {
            error = "إعادة الإرسال متاحة فقط للمهام المعادة.";
            return false;
        }

        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        DueAtUtc = dueAtUtc;
        AssigneeUserId = newAssigneeUserId;
        AssigneeDisplayName = newAssigneeDisplayName;
        CurrentHolderUserId = newAssigneeUserId;
        CurrentHolderDisplayName = newAssigneeDisplayName;
        CustomerId = customerId;
        OrderId = orderId;
        RejectNote = null;
        TransferredAtUtc = null;
        Status = ManagementTaskStatus.Sent;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
        return true;
    }

    public bool TryTransfer(
        int actorUserId,
        int targetUserId,
        string targetDisplayName,
        string? note,
        DateTimeOffset atUtc,
        out string? error)
    {
        error = null;
        if (IsCurrentHolder(actorUserId) == false)
        {
            error = "لست حامل المهمة الحالي.";
            return false;
        }

        if (AssigneeUserId != actorUserId)
        {
            error = "النقل متاح للمكلَّف الأصلي فقط.";
            return false;
        }

        if (Status != ManagementTaskStatus.Accepted)
        {
            error = "لا يمكن نقل المهمة في حالتها الحالية.";
            return false;
        }

        if (targetUserId <= 0)
        {
            error = "مستخدم الفرع مطلوب.";
            return false;
        }

        if (targetUserId == actorUserId)
        {
            error = "لا يمكن النقل إلى نفس المكلَّف.";
            return false;
        }

        CurrentHolderUserId = targetUserId;
        CurrentHolderDisplayName = targetDisplayName;
        TransferredAtUtc = atUtc;
        Status = ManagementTaskStatus.Transferred;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
        return true;
    }

    public bool TryComplete(int actorUserId, DateTimeOffset atUtc, out string? error)
    {
        error = null;
        if (IsCurrentHolder(actorUserId) == false)
        {
            error = "لست حامل المهمة الحالي.";
            return false;
        }

        if (Status != ManagementTaskStatus.Accepted)
        {
            error = "لا يمكن إنجاز المهمة في حالتها الحالية.";
            return false;
        }

        var transferredAndHeldByOther =
            HasActiveTransfer && CurrentHolderUserId != AssigneeUserId;

        if (transferredAndHeldByOther)
        {
            BranchCompletedAtUtc = atUtc;
            CurrentHolderUserId = AssigneeUserId;
            CurrentHolderDisplayName = AssigneeDisplayName;
            Status = ManagementTaskStatus.AwaitingDelegatedConfirm;
            StatusChangedAtUtc = atUtc;
            ConcurrencyStamp++;
            return true;
        }

        if (actorUserId == AssigneeUserId)
            AssigneeCompletedAtUtc = atUtc;
        else
            BranchCompletedAtUtc = atUtc;

        Status = ManagementTaskStatus.Completed;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
        return true;
    }

    public bool TryConfirm(int actorUserId, DateTimeOffset atUtc, out string? error)
    {
        error = null;
        if (HasActiveTransfer == false)
        {
            error = "لا يمكن التصديق بدون نقل سابق.";
            return false;
        }

        if (Status != ManagementTaskStatus.AwaitingDelegatedConfirm)
        {
            error = "التصديق متاح فقط بعد إنجاز الفرع.";
            return false;
        }

        if (AssigneeUserId != actorUserId)
        {
            error = "التصديق متاح للمكلَّف الأصلي فقط.";
            return false;
        }

        ConfirmedAtUtc = atUtc;
        Status = ManagementTaskStatus.Completed;
        StatusChangedAtUtc = atUtc;
        ConcurrencyStamp++;
        return true;
    }

    public bool IsOverdue(DateTimeOffset utcNow, DateOnly iraqToday)
    {
        if (Status == ManagementTaskStatus.Completed)
            return false;

        if (DueAtUtc.HasValue)
            return DueAtUtc.Value < utcNow;

        return DueDate < iraqToday;
    }
}
