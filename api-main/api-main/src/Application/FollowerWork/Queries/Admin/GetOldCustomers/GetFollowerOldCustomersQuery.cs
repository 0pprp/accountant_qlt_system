using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetOldCustomers;

public sealed class GetFollowerOldCustomersQuery : IRequest<Result<FollowerOldCustomersResponse>>
{
    public int FollowerId { get; init; }
    public int? BranchId { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
