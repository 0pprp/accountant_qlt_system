using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetDeclining;

public sealed class GetFollowerDecliningQuery : IRequest<Result<FollowerDecliningResponse>>
{
    public int FollowerId { get; init; }
    public int? BranchId { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
