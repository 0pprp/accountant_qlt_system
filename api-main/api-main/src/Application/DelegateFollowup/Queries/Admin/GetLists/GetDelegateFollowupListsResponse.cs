using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetLists;

public record GetDelegateFollowupListsResponse
{
    public required IReadOnlyList<DelegateFollowupListCardDto> Lists { get; init; }
}
