using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetSalesReview;

public sealed class GetFollowerSalesReviewQuery : IRequest<Result<FollowerSalesReviewWorkspaceDto>>
{
    public int FollowerId { get; init; }
    public int? BranchId { get; init; }
}
