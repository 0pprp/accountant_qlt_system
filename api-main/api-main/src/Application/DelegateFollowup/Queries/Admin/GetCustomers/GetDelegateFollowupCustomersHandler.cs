using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;

namespace Application.DelegateFollowup.Queries.Admin.GetCustomers;

public class GetDelegateFollowupCustomersHandler
    : IRequestHandler<GetDelegateFollowupCustomersQuery, Result<DelegateFollowupCustomersDto>>
{
    private readonly DelegateFollowupDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetDelegateFollowupCustomersHandler(DelegateFollowupDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<DelegateFollowupCustomersDto>> Handle(
        GetDelegateFollowupCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = DelegateFollowupAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        var customers = await _data.GetCustomersAsync(request.ListId, request.BranchId, cancellationToken);
        if (customers is null)
            return DelegateFollowupErrors.ListNotFound;

        return customers;
    }
}
