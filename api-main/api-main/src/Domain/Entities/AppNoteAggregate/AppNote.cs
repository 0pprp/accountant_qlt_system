using Domain.Common;
using Domain.Entities.AppNoteAggregate.Enums;

namespace Domain.Entities.AppNoteAggregate;

/// <summary>
/// ملاحظة تطبيقات → صندوق وارد الإدارة.
/// معزولة عن CustomerNotes / FollowerDelegateNotes / شكاوى المفوض.
/// </summary>
public class AppNote : Entity
{
    private AppNote()
    {
    }

    public AppNote(
        AppNoteSourceApp sourceApp,
        string? sourceType,
        int senderUserId,
        string senderDisplayName,
        string senderRole,
        int? branchId,
        string? branchName,
        string subject,
        string body,
        DateTimeOffset createdAtUtc)
    {
        SourceApp = sourceApp;
        SourceType = sourceType ?? "manager_note";
        SenderUserId = senderUserId;
        SenderDisplayName = senderDisplayName;
        SenderRole = senderRole;
        BranchId = branchId;
        BranchName = branchName;
        Subject = subject;
        Body = body;
        Status = AppNoteStatus.Unread;
        CreatedAtUtc = createdAtUtc;
        ConcurrencyStamp = 0;
    }

    public AppNoteSourceApp SourceApp { get; private set; }
    public string SourceType { get; private set; } = "manager_note";
    public int SenderUserId { get; private set; }
    public string SenderDisplayName { get; private set; } = "";
    public string SenderRole { get; private set; } = "";
    public int? BranchId { get; private set; }
    public string? BranchName { get; private set; }
    public string Subject { get; private set; } = "";
    public string Body { get; private set; } = "";
    public AppNoteStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ReadAtUtc { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    /// <summary>Idempotent: إعادة تعليم المقروء لا تغيّر الأثر.</summary>
    public bool MarkRead(DateTimeOffset atUtc)
    {
        if (Status == AppNoteStatus.Read)
            return false;

        Status = AppNoteStatus.Read;
        ReadAtUtc = atUtc;
        ConcurrencyStamp++;
        return true;
    }
}
