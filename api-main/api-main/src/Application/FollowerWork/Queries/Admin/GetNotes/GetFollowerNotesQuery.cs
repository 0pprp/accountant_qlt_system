using Application.FollowerWork.Common;

namespace Application.FollowerWork.Queries.Admin.GetNotes;

public sealed class GetFollowerNotesQuery : IRequest<Result<FollowerNotesResponse>>
{
    public int FollowerId { get; init; }
    public int? BranchId { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
