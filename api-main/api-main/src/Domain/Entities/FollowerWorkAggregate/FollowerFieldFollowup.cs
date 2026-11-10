using Domain.Common;

namespace Domain.Entities.FollowerWorkAggregate;

/// <summary>
/// متابعة ميدانية للمتابع (Motaba) — مكافئ FollowerFieldFollowups القديم.
/// ليست ManagementTask وليست LegalFollowup.
/// Pending → Completed بملاحظة إجبارية؛ RecordComplete = Add+Complete.
/// </summary>
public class FollowerFieldFollowup : Entity
{
    public const string StatusPending = "Pending";
    public const string StatusCompleted = "Completed";
    public const int MaxNoteLength = 1000;
    public const int MaxNameLength = 200;
    public const int MaxPhoneLength = 50;

    private FollowerFieldFollowup()
    {
    }

    public FollowerFieldFollowup(
        int followerUserId,
        int listId,
        string? listName,
        int customerId,
        string? customerName,
        string? phoneNumber,
        DateTimeOffset createdAtUtc)
    {
        if (followerUserId <= 0 || listId <= 0 || customerId <= 0)
            throw new InvalidOperationException("بيانات النقل غير كاملة");

        FollowerUserId = followerUserId;
        ListId = listId;
        ListName = NormalizeOptional(listName, MaxNameLength);
        CustomerId = customerId;
        CustomerName = NormalizeOptional(customerName, MaxNameLength);
        PhoneNumber = NormalizeOptional(phoneNumber, MaxPhoneLength);
        Status = StatusPending;
        CreatedAtUtc = createdAtUtc;
    }

    public int FollowerUserId { get; private set; }
    public int ListId { get; private set; }
    public string? ListName { get; private set; }
    public int CustomerId { get; private set; }
    public string? CustomerName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string Status { get; private set; } = StatusPending;
    public string? CompletionNote { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public bool IsPending =>
        string.Equals(Status, StatusPending, StringComparison.OrdinalIgnoreCase);

    public void Complete(string? note, DateTimeOffset completedAtUtc)
    {
        if (!IsPending)
            throw new InvalidOperationException("هذه المتابعة مكتملة مسبقاً");

        var normalized = NormalizeNote(note);
        if (string.IsNullOrEmpty(normalized))
            throw new InvalidOperationException("الملاحظة إجبارية قبل اعتماد تمت المتابعة");

        Status = StatusCompleted;
        CompletionNote = normalized;
        CompletedAtUtc = completedAtUtc;
    }

    public static string NormalizeNote(string? note)
    {
        var t = (note ?? string.Empty).Trim();
        if (t.Length > MaxNoteLength)
            t = t[..MaxNoteLength];
        return t;
    }

    private static string? NormalizeOptional(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var t = value.Trim();
        return t.Length > max ? t[..max] : t;
    }
}
