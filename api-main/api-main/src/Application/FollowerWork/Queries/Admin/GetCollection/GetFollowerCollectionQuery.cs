using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetCollection;

public sealed class GetFollowerCollectionQuery : IRequest<Result<FollowerCollectionResponse>>
{
    public int FollowerId { get; init; }
    public int? BranchId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public int? PaymentsPage { get; init; }
    public int? PaymentsPageSize { get; init; }
}
