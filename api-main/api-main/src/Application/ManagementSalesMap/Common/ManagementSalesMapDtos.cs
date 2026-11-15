namespace Application.ManagementSalesMap.Common;

public enum SalesMapStatusFilter
{
    All = 0,
    Continuous = 1,
    Stopped = 2,
    Legal = 3,
}

/// <summary>رمز الحالة على النقطة: 1 مستمر، 2 متوقف، 3 قانونية، null غير محدد.</summary>
public enum SalesMapPointStatusCode
{
    Continuous = 1,
    Stopped = 2,
    Legal = 3,
}

public sealed record SalesMapProvinceCardDto(
    int ProvinceId,
    string ProvinceName,
    int ListCount,
    int SalesWithLocation,
    int SalesWithoutLocation);

public sealed record SalesMapListOptionDto(
    int ListId,
    string ListName,
    int BranchId,
    string Letter,
    int SalesWithLocation,
    int SalesWithoutLocation);

public sealed record SalesMapPointDto(
    int OrderId,
    int CustomerId,
    string CustomerName,
    int ListId,
    string ListName,
    string ListLetter,
    double Latitude,
    double Longitude,
    int? StatusCode,
    string StatusLabel,
    string? RatingTier,
    string? RatingLabel);

public sealed record SalesMapOrderDetailDto(
    int OrderId,
    int CustomerId,
    string CustomerName,
    int ListId,
    string ListName,
    string ListLetter,
    string StatusLabel,
    string? RatingLabel,
    double SellAmount,
    double PaidAmount,
    double RemainingAmount,
    DateOnly? SaleDate,
    DateOnly? LastPaymentDate,
    double? LastPaymentAmount,
    string? LastPaymentDisplay);

public sealed record SalesMapProvincesResponse(IReadOnlyList<SalesMapProvinceCardDto> Items);

public sealed record SalesMapListsResponse(IReadOnlyList<SalesMapListOptionDto> Items);

public sealed record SalesMapPointsResponse(IReadOnlyList<SalesMapPointDto> Items);
