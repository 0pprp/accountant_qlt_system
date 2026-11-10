namespace Application.BranchNewSales.Common;

public sealed record BranchNewSaleItemDto(
    int OrderId,
    int BranchId,
    int CustomerId,
    string CustomerName,
    string? CustomerPhone,
    int? SellerId,
    string? SellerName,
    double SellAmount,
    double DailyInstallmentAmount,
    double PrepaymentAmount,
    DateTimeOffset? SalesCompletedAtUtc,
    DateTimeOffset? BranchManagerReadAtUtc,
    bool IsRead,
    IReadOnlyList<BranchNewSaleLineDto> Items);

public sealed record BranchNewSaleLineDto(
    int ProductId,
    string ProductName,
    int Quantity,
    double UnitSalePrice,
    double LineSalePrice);

public sealed record BranchNewSaleListResponse(
    IReadOnlyList<BranchNewSaleItemDto> Items,
    int TotalCount,
    int UnreadCount,
    int Page,
    int PageSize);

public sealed record BranchNewSaleUnreadCountDto(int UnreadCount);

public static class BranchNewSaleErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "BranchNewSale.Forbidden");
    public static readonly Error NotFound = new("المبيع غير موجود أو خارج الصندوق.", "BranchNewSale.NotFound");
    public static readonly Error BranchScope = new("خارج نطاق فرعك.", "BranchNewSale.BranchScope");
}
