using Domain.Entities.AppNoteAggregate.Enums;

namespace Application.AppNotes.Common;

public sealed record AppNoteSummaryDto(
    int Id,
    string SourceApp,
    string? SourceType,
    string SenderDisplayName,
    string SenderRole,
    int? BranchId,
    string? BranchName,
    string Subject,
    string BodyPreview,
    string Status,
    bool IsUnread,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc,
    int ExpectedVersion);

public sealed record AppNoteDetailDto(
    int Id,
    string SourceApp,
    string? SourceType,
    int SenderUserId,
    string SenderDisplayName,
    string SenderRole,
    int? BranchId,
    string? BranchName,
    string Subject,
    string Body,
    string Status,
    bool IsUnread,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc,
    int ExpectedVersion);

public sealed record AppNoteListResponse(
    IReadOnlyList<AppNoteSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int UnreadCount);

public static class AppNoteLabels
{
    public static string SourceAppArabic(AppNoteSourceApp app) => app switch
    {
        AppNoteSourceApp.Follower => "متابع",
        AppNoteSourceApp.Delegate => "مندوب",
        AppNoteSourceApp.BranchManager => "مدير فرع",
        AppNoteSourceApp.SalesEmployee => "موظف مبيعات",
        _ => app.ToString()
    };
}
