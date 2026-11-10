using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetCustomerPayments;

public class GetDelegateFollowupCustomerPaymentsQuery : IRequest<Result<DelegateFollowupCustomerPaymentsDto>>
{
    public int ListId { get; set; }
    public int BranchId { get; set; }
    public int CustomerId { get; set; }
}
