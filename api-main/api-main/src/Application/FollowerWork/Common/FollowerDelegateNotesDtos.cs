namespace Application.FollowerWork.Common;

public sealed record FollowerDelegateNoteRowDto(
    int NoteId,
    int OrderListId,
    string? ListName,
    int MandobUserId,
    string? MandobName,
    string NoteText,
    int AuthorUserId,
    string? AuthorName,
    string AuthorRole,
    DateTimeOffset CreatedAtUtc,
    string Source);

public sealed record FollowerDelegateNotesResponse(
    IReadOnlyList<FollowerDelegateNoteRowDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    string TimeZoneId,
    DateTimeOffset GeneratedAtUtc,
    int FollowerId,
    string? FollowerName);
