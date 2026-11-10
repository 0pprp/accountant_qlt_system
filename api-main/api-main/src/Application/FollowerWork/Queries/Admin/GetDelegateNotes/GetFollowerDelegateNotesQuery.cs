using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetDelegateNotes;

public sealed class GetFollowerDelegateNotesQuery : IRequest<Result<FollowerDelegateNotesResponse>>
{
    public int FollowerId { get; init; }
    public int? BranchId { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
