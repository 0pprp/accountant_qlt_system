using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetCustomers;

public record GetDelegateFollowupCustomersQuery : IRequest<Result<DelegateFollowupCustomersDto>>
{
    public int ListId { get; init; }
    public int BranchId { get; init; }
}
