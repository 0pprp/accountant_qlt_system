using Application.Common.Interfaces;
using Application.SalesRecords.Common;
using Application.SalesRequests.Common;

namespace Application.SalesRecords.Queries.Admin.List;

public class ListSalesRecordsHandler
    : IRequestHandler<ListSalesRecordsQuery, Result<ListSalesRecordsResponse>>
{
    private readonly SalesRecordsDataService _data;
    private readonly ICurrentUserService _currentUser;

    public ListSalesRecordsHandler(SalesRecordsDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<ListSalesRecordsResponse>> Handle(
        ListSalesRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var authorized = SalesRequestAccess.AuthorizedBranches(_currentUser);
        if (authorized.Count == 0)
            return SalesRecordsErrors.NoBranchScope;

        if (request.BranchId is > 0 && !SalesRequestAccess.CanAccessBranch(_currentUser, request.BranchId.Value))
            return SalesRecordsErrors.BranchForbidden;

        var sortErr = SalesRecordsDataService.ValidateSort(request.Sort);
        if (sortErr is not null)
            return sortErr;

        var scope = request.BranchId is > 0
            ? (IReadOnlyList<int>)[request.BranchId.Value]
            : authorized;

        return await _data.ListAsync(
            scope,
            request.SellerId,
            request.Search,
            request.SaleDateFrom,
            request.SaleDateTo,
            request.Page,
            request.PageSize,
            string.IsNullOrWhiteSpace(request.Sort) ? "saleDate" : request.Sort.Trim(),
            cancellationToken);
    }
}
