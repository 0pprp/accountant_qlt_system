using Domain.Entities.AppNoteAggregate;
using Domain.Entities.AppNoteAggregate.Enums;

namespace Application.AppNotes.Common;

public static class AppNoteMapper
{
    public static AppNoteSummaryDto ToSummary(AppNote n) => new(
        n.Id,
        n.SourceApp.ToString(),
        n.SourceType,
        n.SenderDisplayName,
        n.SenderRole,
        n.BranchId,
        n.BranchName,
        n.Subject,
        n.Body.Length <= 120 ? n.Body : n.Body[..120] + "…",
        n.Status.ToString(),
        n.Status == AppNoteStatus.Unread,
        n.CreatedAtUtc,
        n.ReadAtUtc,
        n.ConcurrencyStamp);

    public static AppNoteDetailDto ToDetail(AppNote n) => new(
        n.Id,
        n.SourceApp.ToString(),
        n.SourceType,
        n.SenderUserId,
        n.SenderDisplayName,
        n.SenderRole,
        n.BranchId,
        n.BranchName,
        n.Subject,
        n.Body,
        n.Status.ToString(),
        n.Status == AppNoteStatus.Unread,
        n.CreatedAtUtc,
        n.ReadAtUtc,
        n.ConcurrencyStamp);
}
