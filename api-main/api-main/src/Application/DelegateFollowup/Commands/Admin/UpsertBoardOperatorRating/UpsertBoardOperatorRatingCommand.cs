using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Commands.Admin.UpsertBoardOperatorRating;

public record UpsertBoardOperatorRatingCommand : IRequest<Result<DelegateFollowupActionResultDto>>
{
    public int ListId { get; init; }
    public int BranchId { get; init; }
    public int Score { get; init; }
    public string? Note { get; init; }
}
