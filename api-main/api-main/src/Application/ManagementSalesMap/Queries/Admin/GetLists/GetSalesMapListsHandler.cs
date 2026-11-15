using Application.Common.Interfaces;
using Application.ManagementSalesMap.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementSalesMap.Queries.Admin.GetLists;

public class GetSalesMapListsHandler
    : IRequestHandler<GetSalesMapListsQuery, Result<SalesMapListsResponse>>
{
    private readonly ManagementSalesMapDataService _data;
    private readonly ICurrentUserService _user;

    public GetSalesMapListsHandler(ManagementSalesMapDataService data, ICurrentUserService user)
    {
        _data = data;
        _user = user;
    }

    public async Task<Result<SalesMapListsResponse>> Handle(
        GetSalesMapListsQuery request,
        CancellationToken cancellationToken)
    {
        if (_user.HasPermission(SeedPermissions.ManagementSalesMap.Read) == false)
            return ManagementSalesMapErrors.Forbidden;

        var (branchIds, _, error) = await _data.ResolveProvinceBranchesAsync(
            _user, request.ProvinceId, cancellationToken);
        if (error is not null)
            return error.Value;

        return await _data.GetListsAsync(branchIds, cancellationToken);
    }
}
