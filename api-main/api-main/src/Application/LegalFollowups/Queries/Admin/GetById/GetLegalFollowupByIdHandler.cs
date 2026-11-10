using Application.Common.Interfaces;
using Application.LegalFollowups.Common;
using Application.SalesRequests.Common;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Queries.Admin.GetById;

public class GetLegalFollowupByIdHandler
    : IRequestHandler<GetLegalFollowupByIdQuery, Result<LegalFollowupCaseRowDto>>
{
    private readonly LegalFollowupDataService _data;
    private readonly ICurrentUserService _currentUser;

    public GetLegalFollowupByIdHandler(LegalFollowupDataService data, ICurrentUserService currentUser)
    {
        _data = data;
        _currentUser = currentUser;
    }

    public async Task<Result<LegalFollowupCaseRowDto>> Handle(
        GetLegalFollowupByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.LegalFollowup.Read) == false)
            return new Error("غير مخول بقراءة المتابعة القانونية.", "LegalFollowup.ReadDenied");

        var scope = SalesRequestAccess.AuthorizedBranches(_currentUser);
        if (scope.Count == 0)
            return new Error("لا فروع مصرّح بها.", "LegalFollowup.NoBranchScope");

        var row = await _data.GetAsync(request.CaseId, scope, cancellationToken);
        if (row is null)
            return new Error("المتابعة غير موجودة ضمن النطاق.", "LegalFollowup.NotFound");

        var docs = await _data.ListDocumentsAsync(request.CaseId, cancellationToken);
        row.Documents = docs;
        return row;
    }
}
