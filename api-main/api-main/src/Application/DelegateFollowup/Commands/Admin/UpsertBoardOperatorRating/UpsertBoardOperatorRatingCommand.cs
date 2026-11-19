using Application.DelegateFollowup.Common;
using Domain.Entities.DelegateFollowupAggregate;

namespace Application.DelegateFollowup.Commands.Admin.UpsertBoardOperatorRating;

public record UpsertBoardOperatorRatingCommand : IRequest<Result<DelegateFollowupActionResultDto>>
{
    public int ListId { get; init; }
    public int BranchId { get; init; }
    public int Score { get; init; }
    public string? Note { get; init; }
    public string RaterSlot { get; init; } = DirectorBoardOperatorListRating.SlotCommittee;
}
