using Domain.Entities.WrongSaleAggregate;

namespace Application.WrongSales.Common;

public sealed record WrongSaleNoticeDto(
    int Id,
    int OrderId,
    int CustomerId,
    int BranchId,
    string CustomerName,
    string? CustomerPhone,
    int Rating,
    string? Note,
    decimal ShopStockEstimatedValue,
    decimal EstimatedDailyRevenue,
    string ReasonBlock,
    string SourceApp,
    string Status,
    string? FollowerName,
    DateTimeOffset CreatedAtUtc,
    bool IsDelegateUnread,
    bool IsBranchUnreviewed,
    DateTimeOffset? DelegateReadAtUtc,
    DateTimeOffset? BranchManagerReviewedAtUtc,
    string? BranchManagerReviewNote,
    int ExpectedVersion);

public sealed record WrongSaleListResponse(
    IReadOnlyList<WrongSaleNoticeDto> Items,
    int TotalCount,
    int UnreadCount,
    int UnreviewedCount,
    int Page,
    int PageSize);

/// <summary>
/// نتيجة تقييم أول تحصيل للمندوب — يطابق مصدر FirstCollectionEvaluation:
/// التقييم يُقبل 1–10؛ WrongSaleNotice يُنشأ فقط عند ≤6.
/// </summary>
public sealed record MandobFirstCollectionEvalResultDto(
    int CustomerId,
    int ListId,
    int Rating,
    string? Note,
    decimal ShopStockEstimatedValue,
    decimal EstimatedDailyRevenue,
    decimal? SaleAmount,
    bool IsWrongSale,
    bool Created,
    int? NoticeId,
    WrongSaleNoticeDto? Notice);

public static class WrongSaleErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "WrongSale.Forbidden");
    public static readonly Error NotFound = new("الإشعار غير موجود.", "WrongSale.NotFound");
    public static readonly Error Concurrency = new("تم تعديل السجل من جهة أخرى.", "WrongSale.Concurrency");
    public static readonly Error BranchScope = new("خارج نطاق فرعك.", "WrongSale.BranchScope");
    public static readonly Error AlreadyReviewed = new("تمت مراجعة هذا الإشعار مسبقاً.", "WrongSale.AlreadyReviewed");
    public static readonly Error NoteRequired = new("ملاحظة المراجعة يجب أن تكون بين 3 و2000 حرف.", "WrongSale.NoteRequired");
}

public static class WrongSaleMapper
{
    public static WrongSaleNoticeDto ToDto(WrongSaleNotice n) =>
        new(
            n.Id, n.OrderId, n.CustomerId, n.BranchId, n.CustomerName, n.CustomerPhone,
            n.Rating, n.Note, n.ShopStockEstimatedValue, n.EstimatedDailyRevenue, n.ReasonBlock,
            n.SourceApp, n.Status.ToString(), n.FollowerName, n.CreatedAtUtc,
            n.DelegateReadAtUtc is null, n.BranchManagerReviewedAtUtc is null,
            n.DelegateReadAtUtc, n.BranchManagerReviewedAtUtc, n.BranchManagerReviewNote,
            n.ConcurrencyStamp);
}
