namespace Application.FollowerWork.Queries.Admin.GetCustomerProfile;

public sealed record GetFollowerCustomerProfileResponse(
    FollowerCustomerGeneralDto General,
    FollowerCustomerAssessmentDto? FollowerAssessment,
    FollowerCustomerAssessmentDto? DelegateAssessment,
    FollowerCustomerSalesEmployeeDto? SalesEmployee,
    string AsOfLocalDate);

public sealed record FollowerCustomerGeneralDto(
    int CustomerId,
    string CustomerName,
    string? PhoneNumber,
    string? ShopName,
    string? Address,
    int ListId,
    string? ListName,
    bool IsLegal,
    string? RatingLabel,
    double SellAmount,
    DateOnly? SaleDate,
    double DailyInstallment,
    string? SellerName,
    double Paid,
    double Remaining,
    DateOnly? LastPaymentDate,
    bool HasLocation,
    double? Latitude,
    double? Longitude);

public sealed record FollowerCustomerAssessmentDto(
    int? Rating,
    string? Note,
    double? ShopStockEstimatedValue,
    double? EstimatedDailyRevenue,
    string? RatedByName,
    DateTimeOffset? RatedAtUtc,
    bool IsLowRisk);

public sealed record FollowerCustomerSalesEmployeeDto(
    int? Rating,
    string? Note,
    string? RatedByName,
    DateTimeOffset? RatedAtUtc,
    string? ShopName,
    string? BusinessType,
    double? StockEstimatedValue,
    double? EstimatedDailyRevenue,
    double? ShopArea);
