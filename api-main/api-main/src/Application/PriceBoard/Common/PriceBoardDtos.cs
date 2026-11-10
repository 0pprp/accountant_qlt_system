namespace Application.PriceBoard.Common;

public sealed record PriceBoardEntryDto(
    int Id,
    string ProductName,
    decimal SalePrice,
    decimal DailyInstallment,
    decimal DownPayment,
    int TargetProvinceId,
    string TargetProvinceName,
    string? CreatedByName,
    string? UpdatedByName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    int ExpectedVersion);

public sealed record PriceBoardListResponse(
    IReadOnlyList<PriceBoardEntryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public static class PriceBoardErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "PriceBoard.Forbidden");
    public static readonly Error NotFound = new("السجل غير موجود.", "PriceBoard.NotFound");
    public static readonly Error ProvinceNotFound = new("المحافظة غير موجودة.", "PriceBoard.ProvinceNotFound");
    public static readonly Error Validation = new("بيانات غير صالحة.", "PriceBoard.Validation");
    public static readonly Error Concurrency = new("تم تعديل السجل من جهة أخرى.", "PriceBoard.Concurrency");
    public static readonly Error BranchDenied = new("خارج نطاق الفرع.", "PriceBoard.BranchDenied");
}

public static class PriceBoardMapper
{
    public static PriceBoardEntryDto ToDto(Domain.Entities.PriceBoardAggregate.PriceBoardEntry e) =>
        new(e.Id, e.ProductName, e.SalePrice, e.DailyInstallment, e.DownPayment,
            e.TargetProvinceId, e.TargetProvinceName, e.CreatedByName, e.UpdatedByName,
            e.CreatedAtUtc, e.UpdatedAtUtc, e.ConcurrencyStamp);
}
