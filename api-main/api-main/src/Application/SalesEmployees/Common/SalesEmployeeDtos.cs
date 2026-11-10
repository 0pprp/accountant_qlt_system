namespace Application.SalesEmployees.Common;

public sealed class ListSalesEmployeesResponse
{
    public required IReadOnlyList<SalesEmployeeCardDto> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required IReadOnlyList<int> BranchScope { get; init; }
}

public sealed class SalesEmployeeCardDto
{
    public required int EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required int PrimaryBranchId { get; init; }
    public required string BranchName { get; init; }
    public required IReadOnlyList<int> VisibleBranchIds { get; init; }
    public required string ShiftStatus { get; init; }
    public DateTimeOffset? ShiftStartedAtUtc { get; init; }
    public int? CurrentShiftId { get; init; }
    public required string LocationStatus { get; init; }
    public DateTimeOffset? LastLocationAtUtc { get; init; }
    /// <summary>إحداثيات القائمة غير مُرجعة عمدًا — تتطلب FollowerLocation/مسار لاحق.</summary>
    public double? LatestRating { get; init; }
    public double? PhantomLatestRating { get; init; }
    public string? RatingLabel { get; init; }
    public int SalesTodayCount { get; init; }
    public int PendingSalesCount { get; init; }
    public bool IsAccountActive { get; init; }
    public IReadOnlyList<SalesEmployeeLinkDto>? Links { get; init; }
}

public sealed class SalesEmployeeLinkDto
{
    public required string Key { get; init; }
    public required string LabelAr { get; init; }
    public required string RouteName { get; init; }
    public required string Path { get; init; }
    public required string RequiredPermission { get; init; }
    public required bool Allowed { get; init; }
}

public sealed class SalesEmployeeRatingSummaryDto
{
    public required int EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required int BranchId { get; init; }
    public required double? LatestRating { get; init; }
    public required double? PhantomLatestRating { get; init; }
    public required string? RatingLabel { get; init; }
    public required int ExcellentCount { get; init; }
    public required int GoodCount { get; init; }
    public required int WeakCount { get; init; }
    public required int RejectedCount { get; init; }
    public required int BadCount { get; init; }
    public required IReadOnlyList<SalesEmployeeTrendPointDto> Points { get; init; }
    public required bool FinancialVisible { get; init; }
    public IReadOnlyList<SalesEmployeeRatingCustomerDto>? Customers { get; init; }
}

public sealed class SalesEmployeeTrendPointDto
{
    public required string Date { get; init; }
    public required double Value { get; init; }
}

public sealed class SalesEmployeeRatingCustomerDto
{
    public required int CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public string? Phone { get; init; }
    public string? DateSale { get; init; }
    public string? LastPaymentDate { get; init; }
    public double? AmountPaid { get; init; }
    public double? AmountRemaining { get; init; }
    public double? AmountTotal { get; init; }
    public required string RatingLabel { get; init; }
    public required bool AmountsRedacted { get; init; }
}
