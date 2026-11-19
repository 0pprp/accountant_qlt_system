namespace Application.FollowerWork.Queries.Admin.GetCustomerProfile;

public sealed class GetFollowerCustomerProfileQuery : IRequest<Result<GetFollowerCustomerProfileResponse>>
{
    public int FollowerId { get; init; }
    public int CustomerId { get; init; }
    public int? BranchId { get; init; }
}
