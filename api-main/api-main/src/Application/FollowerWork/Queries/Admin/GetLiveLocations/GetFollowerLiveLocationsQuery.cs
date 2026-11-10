using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetLiveLocations;

public sealed class GetFollowerLiveLocationsQuery : IRequest<Result<FollowerLiveLocationsResponse>>
{
    public int? BranchId { get; init; }
    public int? FollowerId { get; init; }
}
