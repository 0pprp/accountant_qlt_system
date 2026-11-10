namespace Application.ManagementRatings.Common;

public sealed record ManagementRatingsBranchCardDto(
    int BranchId,
    string Name,
    ManagementRatingsMonthBucketDto? CurrentMonth,
    ManagementRatingsMonthBucketDto? PreviousMonth,
    string Availability);

public sealed record ManagementRatingsMonthBucketDto(
    int YearMonth,
    int NewCustomersCount,
    decimal NewCustomersSaleSum,
    decimal NewCustomersDaySalesSum,
    int ZeroedCustomersCount,
    decimal ZeroedCustomersSaleSum,
    decimal ZeroedCustomersDaySalesSum,
    int NetCustomersCount,
    decimal NetSaleSum,
    decimal NetDaySalesSum);

public sealed record ManagementRatingsBranchesResponse(
    string TimeZone,
    IReadOnlyList<ManagementRatingsBranchCardDto> Branches,
    DateTimeOffset GeneratedAt);

public sealed record ManagementRatingsEntityOptionDto(
    int Id,
    string Name,
    IReadOnlyList<int>? AssignedListIds);

public sealed record ManagementRatingsCatalogResponse(
    int BranchId,
    string BranchName,
    IReadOnlyList<ManagementRatingsEntityOptionDto> Lists,
    IReadOnlyList<ManagementRatingsEntityOptionDto> Followers,
    IReadOnlyList<ManagementRatingsEntityOptionDto> Employees,
    DateTimeOffset GeneratedAt);

public sealed record ManagementRatingsPeriodSummaryResponse(
    int BranchId,
    string Scope,
    int ResourceId,
    ManagementRatingsMonthBucketDto Current,
    ManagementRatingsMonthBucketDto Previous,
    DateTimeOffset GeneratedAt);

public sealed record ManagementRatingsTrendPointDto(string Date, double Value);

public sealed record ManagementRatingsTrendResponse(
    string Scope,
    int ResourceId,
    string Name,
    string MetricLabel,
    DateOnly SaleFrom,
    DateOnly SaleTo,
    DateOnly ChartFrom,
    DateOnly ChartTo,
    string TimeZone,
    IReadOnlyList<ManagementRatingsTrendPointDto> Points,
    double StartValue,
    double EndValue,
    double ChangeAbsolute,
    double ChangePercent,
    string Direction,
    bool PeriodIgnored,
    double? PeriodEndValue,
    string? PeriodRatingLabel,
    int? PeriodCustomerCount,
    string RatingLabel,
    int CustomerCount,
    int ExcellentCount,
    int GoodCount,
    int WeakCount,
    int LaggingCount,
    int BadCount,
    double? PhantomEndValue,
    string? PhantomRatingLabel,
    double? InstallmentSum,
    string Availability,
    string? UnavailableReason,
    DateTimeOffset GeneratedAt);

public sealed record ManagementRatingsCustomerCardDto(
    int CustomerId,
    string CustomerName,
    string? Phone,
    string Level,
    string RatingLabel,
    int Score,
    DateOnly? SaleDate,
    DateOnly? LastPaymentDate,
    double? AmountPaid,
    double? AmountRemaining,
    double? AmountTotal,
    bool IsLegal,
    bool HidePaidRemaining);

public sealed record ManagementRatingsCustomersResponse(
    string Scope,
    int ResourceId,
    string Level,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<ManagementRatingsCustomerCardDto> Items,
    DateTimeOffset GeneratedAt);

public sealed record ManagementRatingsListPaymentsSumResponse(
    int ListId,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal TotalAmount,
    int PaymentCount,
    DateTimeOffset GeneratedAt);
