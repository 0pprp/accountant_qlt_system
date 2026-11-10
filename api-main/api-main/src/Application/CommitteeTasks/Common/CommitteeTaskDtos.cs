namespace Application.CommitteeTasks.Common;

public sealed record CommitteeTaskRowDto(
    int Id,
    string Body,
    bool IsEnabled,
    int SortOrder,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? UpdatedByName,
    int ExpectedVersion);

public sealed record CommitteeTaskAppItemDto(int Id, string Body, int SortOrder);
