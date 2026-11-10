using Domain.Common;

namespace Domain.Entities.ComplaintAggregate;

public class ComplaintMessage : Entity
{
    private ComplaintMessage() { }

    public ComplaintMessage(
        string sourceApp,
        int senderUserId,
        string senderDisplayName,
        string senderRole,
        int? branchId,
        string? branchName,
        string subject,
        string body,
        DateTimeOffset createdAtUtc)
    {
        var b = (body ?? "").Trim();
        if (b.Length is < 10 or > 2000)
            throw new InvalidOperationException("نص الشكوى بين 10 و2000 حرف.");
        SourceApp = (sourceApp ?? "").Trim();
        if (SourceApp.Length == 0) SourceApp = "committee";
        SenderUserId = senderUserId;
        SenderDisplayName = (senderDisplayName ?? "").Trim();
        SenderRole = (senderRole ?? "").Trim();
        BranchId = branchId;
        BranchName = branchName;
        Subject = string.IsNullOrWhiteSpace(subject) ? "شكوى" : subject.Trim();
        Body = b;
        Status = ComplaintStatus.Unread;
        CreatedAtUtc = createdAtUtc;
        ConcurrencyStamp = 0;
    }

    public string SourceApp { get; private set; } = "";
    public int SenderUserId { get; private set; }
    public string SenderDisplayName { get; private set; } = "";
    public string SenderRole { get; private set; } = "";
    public int? BranchId { get; private set; }
    public string? BranchName { get; private set; }
    public string Subject { get; private set; } = "";
    public string Body { get; private set; } = "";
    public ComplaintStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ReadAtUtc { get; private set; }
    public int? ReadByUserId { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    public bool MarkRead(int actorId, DateTimeOffset at, int expectedVersion)
    {
        if (MatchesExpectedVersion(expectedVersion) == false)
            throw new InvalidOperationException("تعارض نسخة السجل.");
        if (Status == ComplaintStatus.Read)
            return false;
        Status = ComplaintStatus.Read;
        ReadAtUtc = at;
        ReadByUserId = actorId;
        ConcurrencyStamp++;
        return true;
    }
}

public enum ComplaintStatus : byte { Unread = 1, Read = 2 }
