using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.ManagementTasks.Common;

public sealed record ManagementTaskListItemDto(
    int Id,
    string Title,
    string Description,
    ManagementTaskStatus Status,
    ManagementTaskPriority Priority,
    DateOnly DueDate,
    DateTimeOffset? DueAtUtc,
    bool IsOverdue,
    int BranchId,
    string? BranchName,
    int CreatedByUserId,
    string? CreatedByDisplayName,
    int AssigneeUserId,
    string AssigneeDisplayName,
    int CurrentHolderUserId,
    string CurrentHolderDisplayName,
    string? RejectNote,
    int? CustomerId,
    int? OrderId,
    int ExpectedVersion,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? StatusChangedAtUtc,
    DateTimeOffset? TransferredAtUtc,
    DateTimeOffset? BranchCompletedAtUtc,
    DateTimeOffset? AssigneeCompletedAtUtc,
    DateTimeOffset? ConfirmedAtUtc);

public sealed record ManagementTaskAttachmentDto(
    int Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset UploadedAtUtc,
    int UploadedByUserId);

public sealed record ManagementTaskEventDto(
    int Id,
    ManagementTaskEventType EventType,
    DateTimeOffset AtUtc,
    int? ActorUserId,
    string? ActorDisplayName,
    int? TargetUserId,
    string? TargetDisplayName,
    string? Note,
    ManagementTaskStatus? PreviousStatus,
    ManagementTaskStatus? NewStatus);

public sealed record ManagementTaskDetailDto(
    ManagementTaskListItemDto Task,
    IReadOnlyList<ManagementTaskEventDto> Events,
    IReadOnlyList<ManagementTaskAttachmentDto> Attachments);

public sealed record ManagementTaskActionResultDto(
    int Id,
    ManagementTaskStatus Status,
    int ExpectedVersion);

public sealed record ManagementTaskAssigneeOptionDto(
    int UserId,
    string DisplayName,
    string Username,
    IReadOnlyList<int> BranchIds);
