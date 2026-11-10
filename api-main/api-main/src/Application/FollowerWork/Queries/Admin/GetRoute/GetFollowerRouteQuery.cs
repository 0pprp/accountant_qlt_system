using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetRoute;

public sealed class GetFollowerRouteQuery : IRequest<Result<FollowerRouteResponse>>
{
    public int FollowerId { get; init; }
    public int? BranchId { get; init; }
    public DateOnly? Date { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
