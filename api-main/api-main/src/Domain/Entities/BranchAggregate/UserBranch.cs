using Domain.Common;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.BranchAggregate;

public class UserBranch : BaseEntity
{
    public UserBranch()
    {
    }
    public UserBranch(int branchId)
    {
        BranchId = branchId;
    }

    public int UserId { get; set; }
    public int BranchId { get; set; }

    public User? User { get; set; }
    public Branch? Branch { get; set; }
}