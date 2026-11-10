namespace Application.Committee.Common;

public record CommitteeFieldFollowupDto
{
    public int Id { get; init; }
    public int FollowerUserId { get; init; }
    public int ListId { get; init; }
    public string? ListName { get; init; }
    public int CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public string? PhoneNumber { get; init; }
    public string Status { get; init; } = "Pending";
    public string? CompletionNote { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
}

public record CommitteeFieldFollowupLatestNoteDto
{
    public int CustomerId { get; init; }
    public string? Note { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
}
