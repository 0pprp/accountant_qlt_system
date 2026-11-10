namespace Application.FollowerWork.Common;

public sealed record FollowerNoteRowDto(
    int NoteId,
    int CustomerId,
    string CustomerName,
    string? Phone,
    int? ListId,
    string? ListName,
    string NoteText,
    string? AuthorName,
    int? AuthorUserId,
    DateTimeOffset CreatedAtUtc,
    string Source);

public sealed record FollowerNotesResponse(
    IReadOnlyList<FollowerNoteRowDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    string TimeZoneId,
    DateTimeOffset GeneratedAtUtc,
    int FollowerId,
    string? FollowerName);
