namespace Application.FollowerWork.Common;

public sealed record FollowerDecliningRowDto(
    int CustomerId,
    string CustomerName,
    string? Phone,
    int ListId,
    string? ListName,
    string FromLabel,
    string ToLabel,
    string? RatingLabel,
    int FromScore,
    int ToScore,
    int DaysBetween,
    DateOnly PriorCaptureDate,
    DateOnly CurrentCaptureDate,
    /// <summary>null = ملاحظات القسم غير منفَّذة بعد (دفعة08) — ليست «لا».</summary>
    bool? HasNote,
    DateTimeOffset? LastNoteAtUtc,
    bool HasSaleAuth);

public sealed record FollowerDecliningResponse(
    IReadOnlyList<FollowerDecliningRowDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    string LookbackRule,
    int LookbackDays,
    int MinScoreDrop,
    IReadOnlyList<string> TargetTransitions,
    string ComparisonMode,
    string NotesAvailability,
    string TimeZoneId,
    string AsOfLocalDate,
    DateTimeOffset GeneratedAtUtc,
    string? FollowerName,
    int FollowerId);
