using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetFollowers;

public sealed class GetFollowerWorkFollowersQuery : IRequest<Result<FollowerWorkFollowersResponse>>
{
    public int? BranchId { get; init; }
    public string? Search { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? Sort { get; init; }
}
