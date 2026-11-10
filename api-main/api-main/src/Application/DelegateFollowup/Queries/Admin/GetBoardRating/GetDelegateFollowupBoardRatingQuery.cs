using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetBoardRating;

public record GetDelegateFollowupBoardRatingQuery : IRequest<Result<DelegateFollowupBoardRatingDto>>
{
    public int ListId { get; init; }
    public int BranchId { get; init; }
    public DateOnly? RatingDate { get; init; }
}
