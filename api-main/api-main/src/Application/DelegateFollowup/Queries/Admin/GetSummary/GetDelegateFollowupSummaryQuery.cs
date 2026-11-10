using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetSummary;

public record GetDelegateFollowupSummaryQuery : IRequest<Result<DelegateFollowupSummaryDto>>
{
    public int ListId { get; init; }
    public int BranchId { get; init; }
}
