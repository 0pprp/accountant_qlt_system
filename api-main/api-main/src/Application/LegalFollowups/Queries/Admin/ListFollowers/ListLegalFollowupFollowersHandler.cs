using Application.Common.Interfaces;
using Application.LegalFollowups.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Queries.Admin.ListFollowers;

public class ListLegalFollowupFollowersHandler
    : IRequestHandler<ListLegalFollowupFollowersQuery, Result<IReadOnlyList<LegalFollowupFollowerOptionDto>>>
{
    private readonly LegalFollowupDataService _data;
    private readonly ICurrentUserService _currentUser;

    public ListLegalFollowupFollowersHandler(LegalFollowupDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<LegalFollowupFollowerOptionDto>>> Handle(
        ListLegalFollowupFollowersQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.LegalFollowup.Create) == false
            && _currentUser.HasPermission(SeedPermissions.LegalFollowup.Read) == false)
            return new Error("غير مخول.", "LegalFollowup.Forbidden");

        var scope = SalesRequestAccess.AuthorizedBranches(_currentUser);
        if (scope.Count == 0)
            return new Error("لا فروع مصرّح بها.", "LegalFollowup.NoBranchScope");

        if (request.BranchId is > 0 && !SalesRequestAccess.CanAccessBranch(_currentUser, request.BranchId.Value))
            return new Error("لا صلاحية على الفرع.", "LegalFollowup.BranchForbidden");

        return (await _data.ListFollowersAsync(scope, request.BranchId, cancellationToken)).ToList();
    }
}
