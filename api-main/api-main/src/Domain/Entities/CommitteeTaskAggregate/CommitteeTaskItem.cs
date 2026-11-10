using Domain.Common;

namespace Domain.Entities.CommitteeTaskAggregate;

/// <summary>
/// نشرة مهام اللجنة — نص مفعّل يظهر في تطبيق مسؤول اللجنة.
/// ليست ManagementTask ولا دورة قبول/إنجاز.
/// </summary>
public class CommitteeTaskItem : Entity
{
    private CommitteeTaskItem()
    {
    }

    public CommitteeTaskItem(string body, bool isEnabled, int sortOrder, int actorUserId, string? actorName, DateTimeOffset atUtc)
    {
        Body = body;
        IsEnabled = isEnabled;
        SortOrder = sortOrder;
        CreatedAtUtc = atUtc;
        UpdatedAtUtc = atUtc;
        UpdatedByUserId = actorUserId;
        UpdatedByName = actorName;
        ConcurrencyStamp = 0;
    }

    public string Body { get; private set; } = "";
    public bool IsEnabled { get; private set; }
    public int SortOrder { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public int UpdatedByUserId { get; private set; }
    public string? UpdatedByName { get; private set; }
    public int ConcurrencyStamp { get; private set; }

    public bool MatchesExpectedVersion(int expected) => ConcurrencyStamp == expected;

    public void Update(string body, bool isEnabled, int actorUserId, string? actorName, DateTimeOffset atUtc)
    {
        Body = body;
        IsEnabled = isEnabled;
        UpdatedAtUtc = atUtc;
        UpdatedByUserId = actorUserId;
        UpdatedByName = actorName;
        ConcurrencyStamp++;
    }
}
