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
    public required List<string> AvailableSections { get; init; }
    public required List<string> SchemaRequiredSections { get; init; }
    public List<string> NotImplementedSections { get; init; } = [];
    public List<string> PartialSections { get; init; } = [];
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
}

public record CustomerAuditNotesDto
{
    public string ImplementationStatus { get; init; } = "schema_required";
    public List<object> Items { get; init; } = [];
}
