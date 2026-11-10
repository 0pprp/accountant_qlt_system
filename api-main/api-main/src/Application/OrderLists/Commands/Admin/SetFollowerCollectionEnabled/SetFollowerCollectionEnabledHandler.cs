using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.OrderLists.Commands.Admin.SetFollowerCollectionEnabled;

public sealed class SetFollowerCollectionEnabledHandler
    : IRequestHandler<SetFollowerCollectionEnabledCommand, Result<SetFollowerCollectionEnabledResultDto>>
{
    private readonly FollowerCollectionDataService _data;
    private readonly ICurrentUserService _currentUser;

    public SetFollowerCollectionEnabledHandler(
        FollowerCollectionDataService data,
        ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<SetFollowerCollectionEnabledResultDto>> Handle(
        SetFollowerCollectionEnabledCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.FollowerWork.CollectionManageEnabled) == false)
            return FollowerWorkErrors.CollectionManageDenied;

        // نطاق الفروع من الجلسة — لا نقبل branchId/followerId من الجسم كتفويض
        var branches = FollowerWorkAccess.ResolveBranchScope(_currentUser, branchId: null);
        if (branches.Count == 0)
            return FollowerWorkErrors.BranchAccessDenied;

        return await _data.SetEnabledAsync(
            request.ListId,
            request.Enabled,
            request.ExpectedVersion,
            branches,
            _currentUser.UserId ?? 0,
            cancellationToken);
    }
}
