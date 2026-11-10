using Domain.Common;

namespace Domain.Entities.FollowerWorkAggregate;

/// <summary>
/// مصادقة بيع ميدانية يكتبها المتابع (Motaba) — ليست موافقة مدير ولا إكمال SalesRequest.
/// وحدة السجل: Order واحد.
/// </summary>
public class FollowerSaleAuthentication : AuditableEntity
{
    private FollowerSaleAuthentication()
    {
    }

    public FollowerSaleAuthentication(
        int orderId,
        int customerId,
        int orderListId,
        int branchId,
        int followerUserId,
        string? followerName,
        int rating,
        string? note,
        decimal shopStockEstimatedValue,
        decimal estimatedDailyRevenue,
        DateTimeOffset authenticatedAtUtc)
    {
        OrderId = orderId;
        CustomerId = customerId;
        OrderListId = orderListId;
        BranchId = branchId;
        FollowerUserId = followerUserId;
        FollowerName = followerName;
        Rating = rating;
        Note = note;
        ShopStockEstimatedValue = shopStockEstimatedValue;
        EstimatedDailyRevenue = estimatedDailyRevenue;
        AuthenticatedAtUtc = authenticatedAtUtc;
    }

    public int OrderId { get; private set; }
    public int CustomerId { get; private set; }
    public int OrderListId { get; private set; }
    public int BranchId { get; private set; }
    public int FollowerUserId { get; private set; }
    public string? FollowerName { get; private set; }
    public int Rating { get; private set; }
    public string? Note { get; private set; }
    public decimal ShopStockEstimatedValue { get; private set; }
    public decimal EstimatedDailyRevenue { get; private set; }
    public DateTimeOffset AuthenticatedAtUtc { get; private set; }
}
