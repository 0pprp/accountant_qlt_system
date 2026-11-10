using Domain.Common;
using Domain.Entities.LegalFollowupAggregate.Enums;

namespace Domain.Entities.LegalFollowupAggregate;

/// <summary>
/// قضية متابعة قانونية يرسلها مدير المبيعات لمتابع — مصدر «الزبائن القديمة».
/// ليست مرادفًا لـ Customer.IsLegal.
/// </summary>
public class LegalFollowupCase : AuditableEntity
{
    private LegalFollowupCase()
    {
    }

    public LegalFollowupCase(
        int? salesRequestId,
        int? customerId,
        int followerUserId,
        string? followerName,
        string customerName,
        string? customerPhone,
        string? customerAddress,
        string? note,
        int createdByUserId,
        string? createdByName,
        DateTimeOffset createdAtUtc)
    {
        SalesRequestId = salesRequestId;
        CustomerId = customerId;
        FollowerUserId = followerUserId;
        FollowerName = followerName;
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        CustomerAddress = customerAddress;
        Note = note;
        Status = LegalFollowupStatus.InProgress;
        CreatedByUserId = createdByUserId;
        CreatedByName = createdByName;
        CreatedAtUtc = createdAtUtc;
        RowVersion = 1;
    }

    public int? SalesRequestId { get; private set; }
    public int? CustomerId { get; private set; }
    public int FollowerUserId { get; private set; }
    public string? FollowerName { get; private set; }
    public LegalFollowupStatus Status { get; private set; }
    public string CustomerName { get; private set; } = "";
    public string? CustomerPhone { get; private set; }
    public string? CustomerAddress { get; private set; }
    public string? Note { get; private set; }
    public string? CompletionNote { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public int CreatedByUserId { get; private set; }
    public string? CreatedByName { get; private set; }
    public int RowVersion { get; private set; }
    public double? ShopLatitude { get; private set; }
    public double? ShopLongitude { get; private set; }

    public bool IsDone => Status == LegalFollowupStatus.Completed;
    public bool IsCancelled => Status == LegalFollowupStatus.Cancelled;

    public static bool CanCancel(LegalFollowupStatus status)
        => status == LegalFollowupStatus.InProgress;

    public void SetShopLocation(double latitude, double longitude, int? expectedVersion)
    {
        if (Status != LegalFollowupStatus.InProgress)
            throw new InvalidOperationException("لا يمكن تحديث الموقع إلا للمتابعات قيد العمل.");
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
            throw new InvalidOperationException("إحداثيات الموقع غير صالحة.");
        if (expectedVersion is int v && v != RowVersion)
            throw new InvalidOperationException("تعارض إصدار المتابعة — أعد التحميل.");

        ShopLatitude = latitude;
        ShopLongitude = longitude;
        RowVersion++;
    }

    public void Cancel(string reason, DateTimeOffset atUtc, int? expectedVersion)
    {
        if (CanCancel(Status) == false)
            throw new InvalidOperationException("يمكن إلغاء المتابعات قيد العمل فقط.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("سبب الإلغاء مطلوب.");
        if (expectedVersion is int v && v != RowVersion)
            throw new InvalidOperationException("تعارض إصدار المتابعة — أعد التحميل.");

        Status = LegalFollowupStatus.Cancelled;
        CompletionNote = reason.Trim();
        CompletedAtUtc = atUtc;
        RowVersion++;
    }

    public void Complete(string? note, DateTimeOffset atUtc, int? expectedVersion = null)
    {
        if (Status != LegalFollowupStatus.InProgress)
            throw new InvalidOperationException("المتابعة ليست قيد العمل.");
        if (expectedVersion is int v && v != RowVersion)
            throw new InvalidOperationException("تعارض إصدار المتابعة — أعد التحميل.");
        Status = LegalFollowupStatus.Completed;
        CompletionNote = string.IsNullOrWhiteSpace(note) ? CompletionNote : note.Trim();
        CompletedAtUtc = atUtc;
        RowVersion++;
    }
}
