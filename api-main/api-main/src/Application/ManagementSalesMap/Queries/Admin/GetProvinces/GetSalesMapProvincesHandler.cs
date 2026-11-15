using Application.Common.Interfaces;
using Application.ManagementSalesMap.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementSalesMap.Queries.Admin.GetProvinces;

public class GetSalesMapProvincesHandler
    : IRequestHandler<GetSalesMapProvincesQuery, Result<SalesMapProvincesResponse>>
{
    private readonly ManagementSalesMapDataService _data;
    private readonly ICurrentUserService _user;

    public GetSalesMapProvincesHandler(ManagementSalesMapDataService data, ICurrentUserService user)
    {
        _data = data;
        _user = user;
    }

    public async Task<Result<SalesMapProvincesResponse>> Handle(
        GetSalesMapProvincesQuery request,
        CancellationToken cancellationToken)
    {
        if (_user.HasPermission(SeedPermissions.ManagementSalesMap.Read) == false)
            return ManagementSalesMapErrors.Forbidden;

        var branches = await _data.AuthorizedBranchIdsAsync(_user, cancellationToken);
        return await _data.GetProvincesAsync(branches, cancellationToken);
    }
}
