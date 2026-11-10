using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetLists;

public record GetDelegateFollowupListsQuery : IRequest<Result<GetDelegateFollowupListsResponse>>
{
    public int? BranchId { get; init; }
}
