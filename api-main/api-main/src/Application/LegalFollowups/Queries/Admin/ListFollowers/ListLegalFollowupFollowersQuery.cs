using Application.LegalFollowups.Common;

namespace Application.LegalFollowups.Queries.Admin.ListFollowers;

public sealed class ListLegalFollowupFollowersQuery
    : IRequest<Result<IReadOnlyList<LegalFollowupFollowerOptionDto>>>
{
    public int? BranchId { get; init; }
}
