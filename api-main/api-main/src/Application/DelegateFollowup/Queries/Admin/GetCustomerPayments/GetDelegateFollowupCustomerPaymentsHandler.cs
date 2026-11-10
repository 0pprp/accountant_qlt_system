using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetCustomerPayments;

public class GetDelegateFollowupCustomerPaymentsHandler
    : IRequestHandler<GetDelegateFollowupCustomerPaymentsQuery, Result<DelegateFollowupCustomerPaymentsDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly DelegateFollowupDataService _data;

    public GetDelegateFollowupCustomerPaymentsHandler(
        ICurrentUserService currentUser,
        DelegateFollowupDataService data)
    {
        _currentUser = currentUser;
        _data = data;
    }

    public async Task<Result<DelegateFollowupCustomerPaymentsDto>> Handle(
        GetDelegateFollowupCustomerPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = DelegateFollowupAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        if (request.CustomerId <= 0)
            return DelegateFollowupErrors.ListNotFound;

        var dto = await _data.GetCustomerPaymentsAsync(
            request.ListId,
            request.BranchId,
            request.CustomerId,
            cancellationToken);
        if (dto is null)
            return DelegateFollowupErrors.ListNotFound;

        return dto;
    }
}
