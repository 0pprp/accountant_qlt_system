using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.SalesShiftAggregate;

public class SalesShift : Entity
{
    public SalesShift(int userId, int branchId, DateTimeOffset startedAtUtc)
    {
        UserId = userId;
        BranchId = branchId;
        StartedAtUtc = startedAtUtc;
    }

    public int UserId { get; set; }
    public int BranchId { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? EndedAtUtc { get; set; }

    public User? User { get; set; }
    public Branch? Branch { get; set; }

    public bool IsActive => EndedAtUtc is null;

    public void End(DateTimeOffset endedAtUtc)
    {
        EndedAtUtc = endedAtUtc;
    }
}
