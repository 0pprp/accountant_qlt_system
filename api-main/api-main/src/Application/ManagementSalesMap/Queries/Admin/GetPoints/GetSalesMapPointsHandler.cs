using Application.Common.Interfaces;
using Application.ManagementSalesMap.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementSalesMap.Queries.Admin.GetPoints;

public class GetSalesMapPointsHandler
    : IRequestHandler<GetSalesMapPointsQuery, Result<SalesMapPointsResponse>>
{
    private readonly ManagementSalesMapDataService _data;
    private readonly ICurrentUserService _user;

    public GetSalesMapPointsHandler(ManagementSalesMapDataService data, ICurrentUserService user)
    {
        _data = data;
        _user = user;
    }

    public async Task<Result<SalesMapPointsResponse>> Handle(
        GetSalesMapPointsQuery request,
        CancellationToken cancellationToken)
    {
        if (_user.HasPermission(SeedPermissions.ManagementSalesMap.Read) == false)
            return ManagementSalesMapErrors.Forbidden;

        var (branchIds, _, error) = await _data.ResolveProvinceBranchesAsync(
            _user, request.ProvinceId, cancellationToken);
        if (error is not null)
            return error.Value;

        return await _data.GetPointsAsync(
            branchIds,
            request.ListIds,
            request.Status,
            cancellationToken);
    }
}
