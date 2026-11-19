namespace Application.FollowerWork.Queries.Admin.GetShopCapture;

public sealed class GetFollowerShopCaptureQuery : IRequest<Result<GetFollowerShopCaptureResponse>>
{
    public int BranchId { get; init; }
    public int? FollowerId { get; init; }
}
