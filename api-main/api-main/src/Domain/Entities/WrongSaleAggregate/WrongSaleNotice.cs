using Domain.Common;

namespace Domain.Entities.WrongSaleAggregate;

/// <summary>
/// إشعار مبيع خاطئ — مصادر موثوقة: مصادقة متابع ≤6 أو تقييم أول تحصيل مندوب ≤6.
/// منفصل عن طلب استثناء SalesRequest وعن رفض الطلب.
/// </summary>
public class WrongSaleNotice : Entity
{
    public const string SourceFollowerAuth = "follower-auth";
    public const string SourceMandobFirstCollection = "mandob-first-collection";

    private WrongSaleNotice()
    {
    }

    public WrongSaleNotice(
        int orderId,
        int customerId,
        int branchId,
        int followerUserId,
        string? followerName,
        string? customerName,
        string? customerPhone,
        int rating,
        string? note,
        decimal shopStockEstimatedValue,
        decimal estimatedDailyRevenue,
        string reasonBlock,
        DateTimeOffset atUtc,
        string sourceApp = SourceFollowerAuth)
    {
        OrderId = orderId;
        CustomerId = customerId;
        BranchId = branchId;
        FollowerUserId = followerUserId;
        FollowerName = followerName;
        CustomerName = customerName ?? "";
        CustomerPhone = customerPhone;
        Rating = rating;
        Note = note;
        ShopStockEstimatedValue = shopStockEstimatedValue;
        EstimatedDailyRevenue = estimatedDailyRevenue;
        ReasonBlock = reasonBlock;
        SourceApp = string.IsNullOrWhiteSpace(sourceApp) ? SourceFollowerAuth : sourceApp.Trim();
        Status = WrongSaleNoticeStatus.Pending;
        CreatedAtUtc = atUtc;
        ConcurrencyStamp = 0;
    }

    public int OrderId { get; private set; }
    public int CustomerId { get; private set; }
    public int BranchId { get; private set; }
    public int FollowerUserId { get; private set; }
    public string? FollowerName { get; private set; }
    public string CustomerName { get; private set; } = "";
    public string? CustomerPhone { get; private set; }
    public int Rating { get; private set; }
    public string? Note { get; private set; }
    public decimal ShopStockEstimatedValue { get; private set; }
    public decimal EstimatedDailyRevenue { get; private set; }
    public string ReasonBlock { get; private set; } = "";
    /// <summary>follower-auth | mandob-first-collection — من السيرفر فقط.</summary>
    public string SourceApp { get; private set; } = SourceFollowerAuth;
    public WrongSaleNoticeStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? DelegateReadAtUtc { get; private set; }
    public int? DelegateReadByUserId { get; private set; }
    public DateTimeOffset? BranchManagerReviewedAtUtc { get; private set; }
    public int? BranchManagerReviewedByUserId { get; private set; }
    public string? BranchManagerReviewNote { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    /// <summary>تعليمليم مقروء للمفوض — لا يغيّر حالة الأعمال التجارية.</summary>
    public bool MarkDelegateRead(int actorId, DateTimeOffset at, int expectedVersion)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        if (DelegateReadAtUtc is not null)
            return false;
        DelegateReadAtUtc = at;
        DelegateReadByUserId = actorId;
        ConcurrencyStamp++;
        return true;
    }

    /// <summary>مراجعة إدارة الفرع — مستقلة عن قراءة المفوض. الملاحظة 3–2000 حرفًا.</summary>
    public bool MarkBranchReviewed(int actorId, string? note, DateTimeOffset at, int expectedVersion)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        if (BranchManagerReviewedAtUtc is not null)
            throw new InvalidOperationException("تمت المراجعة مسبقاً.");
        var trimmed = (note ?? "").Trim();
        if (trimmed.Length is < 3 or > 2000)
            throw new InvalidOperationException("ملاحظة المراجعة يجب أن تكون بين 3 و2000 حرف.");
        BranchManagerReviewedAtUtc = at;
        BranchManagerReviewedByUserId = actorId;
        BranchManagerReviewNote = trimmed;
        if (Status == WrongSaleNoticeStatus.Pending)
            Status = WrongSaleNoticeStatus.BranchReviewed;
        ConcurrencyStamp++;
        return true;
    }
}

public enum WrongSaleNoticeStatus : byte
{
    Pending = 1,
    BranchReviewed = 2
}
