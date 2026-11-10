using Domain.Common;
using Domain.Entities.WaiverAggregate.Enums;

namespace Domain.Entities.WaiverAggregate;

/// <summary>
/// طلب تنازل — دورة فرع → مفوض → محاسب.
/// الأثر المالي عند Apply فقط: خفض SellAmount بسقف المتبقي.
/// </summary>
public class CustomerWaiverRequest : Entity
{
    private CustomerWaiverRequest()
    {
    }

    public CustomerWaiverRequest(
        int customerId,
        int branchId,
        int? orderId,
        int requesterUserId,
        string? requesterName,
        string note,
        DateTimeOffset createdAtUtc)
    {
        CustomerId = customerId;
        BranchId = branchId;
        OrderId = orderId;
        RequesterUserId = requesterUserId;
        RequesterName = requesterName;
        RequestNote = note;
        Status = WaiverRequestStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        ConcurrencyStamp = 0;
        Applied = false;
    }

    public int CustomerId { get; private set; }
    public int BranchId { get; private set; }
    public int? OrderId { get; private set; }
    public int RequesterUserId { get; private set; }
    public string? RequesterName { get; private set; }
    public string RequestNote { get; private set; } = "";
    public WaiverRequestStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public int? DecidedByUserId { get; private set; }
    public string? DecidedByName { get; private set; }
    public string? DecisionNote { get; private set; }
    public double? ApprovedAmount { get; private set; }
    public DateTimeOffset? AppliedAtUtc { get; private set; }
    public int? AppliedByUserId { get; private set; }
    public string? AppliedByName { get; private set; }
    public double? AppliedAmount { get; private set; }
    public bool Applied { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    public void Reject(int actorId, string? actorName, string? note, int expectedVersion, DateTimeOffset at)
    {
        Ensure(WaiverRequestStatus.Pending, expectedVersion);
        if (actorId == RequesterUserId)
            throw new InvalidOperationException("لا يجوز لصاحب الطلب اعتماد أو رفض طلبه.");
        Status = WaiverRequestStatus.Rejected;
        DecidedAtUtc = at;
        DecidedByUserId = actorId;
        DecidedByName = actorName;
        DecisionNote = note;
        ConcurrencyStamp++;
    }

    public void ApproveByDelegate(int actorId, string? actorName, double amount, string? note, int expectedVersion, DateTimeOffset at)
    {
        Ensure(WaiverRequestStatus.Pending, expectedVersion);
        if (actorId == RequesterUserId)
            throw new InvalidOperationException("لا يجوز لصاحب الطلب اعتماد طلبه.");
        if (amount <= 0)
            throw new InvalidOperationException("مبلغ التنازل يجب أن يكون أكبر من صفر.");
        Status = WaiverRequestStatus.AwaitingAccountant;
        ApprovedAmount = amount;
        DecidedAtUtc = at;
        DecidedByUserId = actorId;
        DecidedByName = actorName;
        DecisionNote = note;
        ConcurrencyStamp++;
    }

    /// <summary>Idempotent: إعادة التطبيق بعد Applied لا تكرر الأثر.</summary>
    public bool ApplyByAccountant(int actorId, string? actorName, double cappedAmount, int expectedVersion, DateTimeOffset at)
    {
        if (Applied && Status == WaiverRequestStatus.Approved)
            return false;

        Ensure(WaiverRequestStatus.AwaitingAccountant, expectedVersion);
        if (cappedAmount <= 0)
            throw new InvalidOperationException("لا يوجد متبقي قابل للتنازل.");

        Status = WaiverRequestStatus.Approved;
        Applied = true;
        AppliedAmount = cappedAmount;
        AppliedAtUtc = at;
        AppliedByUserId = actorId;
        AppliedByName = actorName;
        ConcurrencyStamp++;
        return true;
    }

    private void Ensure(WaiverRequestStatus required, int expectedVersion)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        if (Status != required)
            throw new InvalidOperationException("انتقال غير مسموح من الحالة الحالية.");
    }
}

public static class WaiverDiscount
{
    public static double Cap(double saleTotal, double paidTotal, double requested)
    {
        if (requested <= 0)
            throw new ArgumentOutOfRangeException(nameof(requested), "مبلغ التنازل يجب أن يكون أكبر من صفر");
        var remaining = Math.Max(0, saleTotal - Math.Max(0, paidTotal));
        return Math.Min(requested, remaining);
    }
}
