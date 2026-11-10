namespace Application.FollowerWork.Common;

public sealed record FollowerSalesReviewListCardDto(
    int ListId,
    string ListName,
    double? PhantomRating,
    string? PhantomRatingLabel,
    double? PhantomRatingWeekAgo,
    string ScoreTrend,
    int RatedCustomerCount);

public sealed record FollowerSalesReviewRatingBucketDto(
    string LevelKey,
    string Label,
    int CustomerCount,
    int CustomerCountYesterday,
    string CountTrend);

public sealed record FollowerSalesReviewSaleRowDto(
    int OrderId,
    int CustomerId,
    string CustomerName,
    string? Phone,
    int ListId,
    string? ListName,
    bool Authenticated,
    DateTimeOffset? AuthenticatedAtUtc,
    int? AuthRating,
    string? AuthNote,
    decimal? ShopStockEstimatedValue,
    decimal? EstimatedDailyRevenue,
    int? ShopRating,
    string? ShopRatingNote,
    decimal? DelegateShopStockEstimatedValue,
    decimal? DelegateEstimatedDailyRevenue,
    decimal? EmployeeShopStockEstimatedValue,
    decimal? EmployeeEstimatedDailyRevenue,
    string? CurrentRatingLabel,
    double? SalePrice,
    DateOnly? SaleDate,
    double? DailyInstallment,
    string? SellerName,
    double? PaidAmount,
    double? RemainingAmount,
    bool IsLowAuthRisk);

public sealed record FollowerSalesReviewIdentityDto(int FollowerId, string FollowerName);

public sealed record FollowerSalesReviewWorkspaceDto(
    FollowerSalesReviewIdentityDto? Follower,
    IReadOnlyList<FollowerSalesReviewListCardDto> ListCards,
    IReadOnlyList<FollowerSalesReviewRatingBucketDto> RatingBuckets,
    int RatedCustomerCount,
    int RemainingLe100Count,
    int RemainingLt200Count,
    string RemainingLe100Trend,
    string RemainingLt200Trend,
    int PendingSalesCount,
    int AuthenticatedSalesCount,
    IReadOnlyList<FollowerSalesReviewSaleRowDto> SalesRows,
    string TimeZone,
    string AsOfLocalDate,
    DateTimeOffset GeneratedAt);

public sealed record FollowerRemainingCustomerDto(
    int CustomerId,
    string CustomerName,
    string? Phone,
    int ListId,
    string? ListName,
    double AmountRemaining,
    string? RatingLabel);

public sealed record FollowerRemainingCustomersResponse(
    string Bucket,
    IReadOnlyList<FollowerRemainingCustomerDto> Items,
    int TotalCount,
    string TimeZone,
    string AsOfLocalDate,
    DateTimeOffset GeneratedAt);
