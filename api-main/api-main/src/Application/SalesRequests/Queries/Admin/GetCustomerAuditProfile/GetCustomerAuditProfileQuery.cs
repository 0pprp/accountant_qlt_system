namespace Application.SalesRequests.Queries.Admin.GetCustomerAuditProfile;

public record GetCustomerAuditProfileQuery(int CustomerId, string? Sections)
    : IRequest<Result<GetCustomerAuditProfileResponse>>;

public record GetCustomerAuditProfileResponse
{
    public required CustomerAuditHeaderDto Header { get; init; }
    public CustomerAuditOverviewDto? Overview { get; init; }
    public CustomerAuditAccountsDto? Accounts { get; init; }
    public List<CustomerAuditSaleDto>? Sales { get; init; }
    public CustomerAuditDocumentsDto? Documents { get; init; }
    public CustomerAuditShopDto? Shop { get; init; }
    public CustomerAuditNotesDto? Notes { get; init; }
    public CustomerAuditRatingsAndLocationDto? RatingsAndLocation { get; init; }
    public required List<string> AvailableSections { get; init; }
    public required List<string> SchemaRequiredSections { get; init; }
    public List<string> NotImplementedSections { get; init; } = [];
    public List<string> PartialSections { get; init; } = [];
}

public record CustomerAuditRatingsAndLocationDto
{
    public CustomerAuditStaffRatingDto? DelegateRating { get; init; }
    public CustomerAuditStaffRatingDto? CommitteeFieldRating { get; init; }
    public CustomerAuditShopSurveyDto? ShopSurvey { get; init; }
    public double? GpsLatitude { get; init; }
    public double? GpsLongitude { get; init; }
    public bool HasGps { get; init; }
    public double? LatestOrderLatitude { get; init; }
    public double? LatestOrderLongitude { get; init; }
    public List<CustomerAuditRatingSnapshotDto> Snapshots { get; init; } = [];
    public List<CustomerAuditLocationDto> Locations { get; init; } = [];
    public List<CustomerAuditPhotoDto> Photos { get; init; } = [];
}

public record CustomerAuditLocationDto
{
    public required string Source { get; init; }
    public required string SourceLabel { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double? AccuracyMeters { get; init; }
    public double? Confidence { get; init; }
    public DateTimeOffset? CapturedAtUtc { get; init; }
}

public record CustomerAuditPhotoDto
{
    public int Id { get; init; }
    public string? ContentType { get; init; }
    public DateTimeOffset? CapturedAtUtc { get; init; }
}

public record CustomerAuditStaffRatingDto
{
    public required string Role { get; init; }
    public required string Title { get; init; }
    public int? Score { get; init; }
    public string? Note { get; init; }
    public string? RatedByName { get; init; }
    public DateTimeOffset? RatedAtUtc { get; init; }
    public int? OrderId { get; init; }
    public double? ShopStockEstimatedValue { get; init; }
    public double? EstimatedDailyRevenue { get; init; }
    public double? ContextAmount { get; init; }
    public string? Occasion { get; init; }
    public string? OccasionLabel { get; init; }
    public bool IsWrongSale { get; init; }
}

public record CustomerAuditShopSurveyDto
{
    public required string Title { get; init; }
    public string? ShopName { get; init; }
    public string? BusinessType { get; init; }
    public double? StockEstimatedValue { get; init; }
    public double? EstimatedDailyRevenue { get; init; }
    public double? ShopLength { get; init; }
    public double? ShopWidth { get; init; }
    public double? ShopArea { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public int? PhotoId { get; init; }
    public DateTimeOffset? CreatedAtUtc { get; init; }
}

public record CustomerAuditHeaderDto
{
    public int Id { get; init; }
    public required string FullName { get; init; }
    public string? Phone { get; init; }
    public string? BranchName { get; init; }
    public string? StatusLabel { get; init; }
}

public record CustomerAuditOverviewDto
{
    public string? MotherName { get; init; }
    public string? NationalCode { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? Address { get; init; }
    public string? BusinessType { get; init; }
    public string? RatingLabel { get; init; }
    public double? SellTotal { get; init; }
    public double? PaidTotal { get; init; }
    public double? Remaining { get; init; }
}

public record CustomerAuditAccountsDto
{
    public required List<CustomerAuditAccountRowDto> Rows { get; init; }
}

public record CustomerAuditAccountRowDto
{
    public int OrderId { get; init; }
    public double? SellAmount { get; init; }
    public double? PaidAmount { get; init; }
    public double? Remaining { get; init; }
    public required List<CustomerAuditPaymentRowDto> Payments { get; init; }
}

public record CustomerAuditPaymentRowDto
{
    public int Id { get; init; }
    public DateOnly Date { get; init; }
    public double Amount { get; init; }
    public string? ReceiptNumber { get; init; }
    public string? Note { get; init; }
}

public record CustomerAuditSaleDto
{
    public int Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string? SellerName { get; init; }
    public string? OrderListName { get; init; }
    public double? SellAmount { get; init; }
    public double? PrepaymentAmount { get; init; }
    public required List<string> Products { get; init; }
}

public record CustomerAuditDocumentsDto
{
    public required List<CustomerAuditDocumentDto> CustomerDocuments { get; init; }
    public required List<CustomerAuditDocumentDto> SaleDocuments { get; init; }
}

public record CustomerAuditDocumentDto
{
    public int Id { get; init; }
    public required string OwnerType { get; init; }
    public int? OwnerId { get; init; }
    public required string OriginalFileName { get; init; }
    public string? RelativePath { get; init; }
    public string? Type { get; init; }
}

public record CustomerAuditShopDto
{
    public string? Address { get; init; }
    public string? Location { get; init; }
    public string ImplementationStatus { get; init; } = "schema_required";
    public string? ShopName { get; init; }
    public string? BusinessType { get; init; }
    public double? StockEstimatedValue { get; init; }
    public double? EstimatedDailyRevenue { get; init; }
    public double? ShopLength { get; init; }
    public double? ShopWidth { get; init; }
    public double? ShopArea { get; init; }
    public string? ShopImageKey { get; init; }
    public int? ShopPhotoId { get; init; }
    public string? LocationSource { get; init; }
}

public record CustomerAuditRatingSnapshotDto
{
    public required string Origin { get; init; }
    public string? RatingLabel { get; init; }
    public double? Score { get; init; }
    public DateOnly CaptureDate { get; init; }
}

public record CustomerAuditNotesDto
{
    public string ImplementationStatus { get; init; } = "schema_required";
    public List<object> Items { get; init; } = [];
}
