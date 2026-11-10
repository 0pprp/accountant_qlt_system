using Application.SalesRequests.Common;

namespace Application.SalesExcelSearch.Common;

public sealed record SalesExcelSearchSaleDto(
    int SaleId,
    DateTime? SaleDate,
    double? SaleAmount,
    double? ReceiptsTotal,
    double? AmountRemaining,
    bool? AccountZero);

public sealed record SalesExcelSearchMatchDto(
    string ResultKey,
    int BranchId,
    string? BranchName,
    int CustomerId,
    string FullName,
    string? Phone,
    string? Address,
    string? MandobName,
    int? MandobId,
    double? AmountTotalSales,
    double? ReceiptsTotal,
    double? AmountRemaining,
    IReadOnlyList<SalesExcelSearchSaleDto> Sales);

public sealed record SalesExcelSearchQueryDto(
    int RowNumber,
    string RequestedName,
    string SearchKey,
    bool UsedFamilySearch,
    bool Found,
    int MatchCount,
    bool Truncated,
    string? Warning,
    IReadOnlyList<SalesExcelSearchMatchDto> Matches);

public sealed record SalesExcelSearchResponse(
    bool ReadOnly,
    int NameCount,
    int FoundCount,
    int MissingCount,
    IReadOnlyList<SalesExcelSearchQueryDto> Queries);

public static class SalesExcelSearchErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "SalesExcelSearch.Forbidden");
    public static readonly Error BranchScope = new("خارج نطاق فرعك.", "SalesExcelSearch.BranchScope");
    public static readonly Error TooManyNames = new(
        $"عدد الأسماء أكبر من الحد المسموح ({SalesCustomerNameMatch.MaxNames}).",
        "SalesExcelSearch.TooManyNames");
}
