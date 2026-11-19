namespace Application.FollowerWork.Queries.Admin.GetShopCapture;

public sealed record GetFollowerShopCaptureResponse(
    int TotalCustomers,
    int WithLocation,
    int WithPhoto,
    int Complete,
    IReadOnlyList<FollowerShopCaptureListDto> Lists,
    IReadOnlyList<FollowerShopCaptureCustomerDto> Customers,
    string AsOfLocalDate);

public sealed record FollowerShopCaptureListDto(
    int ListId,
    string ListName,
    int FollowerId,
    string? FollowerName,
    int TotalCustomers,
    int WithLocation,
    int WithPhoto,
    int Complete);

public sealed record FollowerShopCaptureCustomerDto(
    int CustomerId,
    string CustomerName,
    string? ShopName,
    string? PhoneNumber,
    int ListId,
    double AmountRemaining,
    bool HasLocation,
    double? Latitude,
    double? Longitude,
    bool HasPhoto,
    int? PhotoId,
    string? PhotoSource,
    DateTimeOffset? PhotoCapturedAtUtc);
