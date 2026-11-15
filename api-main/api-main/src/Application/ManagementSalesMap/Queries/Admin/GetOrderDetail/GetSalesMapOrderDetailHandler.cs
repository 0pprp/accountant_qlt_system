using Application.Common.Interfaces;
using Application.ManagementSalesMap.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementSalesMap.Queries.Admin.GetOrderDetail;

public class GetSalesMapOrderDetailHandler
    : IRequestHandler<GetSalesMapOrderDetailQuery, Result<SalesMapOrderDetailDto>>
{
    private readonly ManagementSalesMapDataService _data;
    private readonly ICurrentUserService _user;

    public GetSalesMapOrderDetailHandler(ManagementSalesMapDataService data, ICurrentUserService user)
    {
        _data = data;
        _user = user;
    }

    public async Task<Result<SalesMapOrderDetailDto>> Handle(
        GetSalesMapOrderDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (_user.HasPermission(SeedPermissions.ManagementSalesMap.Read) == false)
            return ManagementSalesMapErrors.Forbidden;

        var branches = await _data.AuthorizedBranchIdsAsync(_user, cancellationToken);
        return await _data.GetOrderDetailAsync(branches, request.OrderId, cancellationToken);
    }
}
