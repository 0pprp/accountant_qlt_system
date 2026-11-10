using Application.Common.Interfaces;
using Application.LegalFollowups.Common;
using Application.SalesRequests.Common;
using Domain.Entities.LegalFollowupAggregate.Enums;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Queries.Admin.List;

public class ListLegalFollowupsHandler
    : IRequestHandler<ListLegalFollowupsQuery, Result<ListLegalFollowupsResponse>>
{
    private readonly LegalFollowupDataService _data;
    private readonly ICurrentUserService _currentUser;

    public ListLegalFollowupsHandler(LegalFollowupDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<ListLegalFollowupsResponse>> Handle(
        ListLegalFollowupsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.LegalFollowup.Read) == false)
            return new Error("غير مخول بقراءة المتابعة القانونية.", "LegalFollowup.ReadDenied");

        var authorized = SalesRequestAccess.AuthorizedBranches(_currentUser);
        if (authorized.Count == 0)
            return new Error("لا فروع مصرّح بها.", "LegalFollowup.NoBranchScope");

        if (request.BranchId is > 0 && !SalesRequestAccess.CanAccessBranch(_currentUser, request.BranchId.Value))
            return new Error("لا صلاحية على الفرع.", "LegalFollowup.BranchForbidden");

        LegalFollowupStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<LegalFollowupStatus>(request.Status.Trim(), ignoreCase: true, out var parsed)
                || parsed == LegalFollowupStatus.Cancelled)
                return new Error("حالة غير مسموحة للعرض.", "LegalFollowup.InvalidStatus");
            status = parsed;
        }

        var scope = request.BranchId is > 0
            ? (IReadOnlyList<int>)[request.BranchId.Value]
            : authorized;

        return await _data.ListAsync(
            scope,
            status,
            request.Search,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
