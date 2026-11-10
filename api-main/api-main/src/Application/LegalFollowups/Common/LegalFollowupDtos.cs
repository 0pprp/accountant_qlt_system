namespace Application.LegalFollowups.Common;

public sealed class ListLegalFollowupsResponse
{
    public required IReadOnlyList<LegalFollowupCaseRowDto> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required IReadOnlyList<int> BranchScope { get; init; }
}

public sealed class LegalFollowupCaseRowDto
{
    public required int Id { get; init; }
    public int? SalesRequestId { get; init; }
    public int? CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerPhone { get; init; }
    public string? CustomerAddress { get; init; }
    public required int FollowerUserId { get; init; }
    public string? FollowerName { get; init; }
    public required string Status { get; init; }
    public required string StatusLabel { get; init; }
    public string? Note { get; init; }
    public string? CompletionNote { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public required int RowVersion { get; init; }
    public required bool CanCancel { get; init; }
    public int? BranchId { get; init; }
    public double? ShopLatitude { get; init; }
    public double? ShopLongitude { get; init; }
    public IReadOnlyList<LegalFollowupDocumentDto>? Documents { get; set; }
}

public sealed class LegalFollowupDocumentDto
{
    public required int Id { get; init; }
    public required string DocumentType { get; init; }
    public required string TypeLabel { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long SizeBytes { get; init; }
    public required DateTimeOffset UploadedAtUtc { get; init; }
}

public sealed class LegalFollowupFollowerOptionDto
{
    public required int FollowerUserId { get; init; }
    public required string FollowerName { get; init; }
    public required int BranchId { get; init; }
}
