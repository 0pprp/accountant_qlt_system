using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetRemainingCustomers;

public sealed class GetFollowerRemainingCustomersQuery : IRequest<Result<FollowerRemainingCustomersResponse>>
{
    public int FollowerId { get; init; }
    public int? BranchId { get; init; }
    public string? Bucket { get; init; }
}
