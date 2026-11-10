using Domain.Common;

namespace Domain.Entities.SalesEmployeeAggregate;

/// <summary>
/// تكليف صريح لموظف المبيعات على فرع — ليس دور Mandob وحده.
/// EndsAtUtc = null يعني ساري.
/// </summary>
public class SalesEmployeeAssignment : Entity
{
    private SalesEmployeeAssignment()
    {
    }

    public SalesEmployeeAssignment(
        int userId,
        int branchId,
        DateTimeOffset startsAtUtc,
        int? createdByUserId,
        string? note = null)
    {
        UserId = userId;
        BranchId = branchId;
        StartsAtUtc = startsAtUtc;
        CreatedAtUtc = startsAtUtc;
        CreatedByUserId = createdByUserId;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public int UserId { get; private set; }
    public int BranchId { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset? EndsAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public int? CreatedByUserId { get; private set; }
    public string? Note { get; private set; }

    public bool IsActiveAt(DateTimeOffset utc) =>
        StartsAtUtc <= utc && (EndsAtUtc is null || EndsAtUtc > utc);

    public void End(DateTimeOffset endsAtUtc)
    {
        if (EndsAtUtc is not null)
            return;
        EndsAtUtc = endsAtUtc;
    }
}
