using Application.Common.Interfaces;
using Application.SalesFollowup.Common;

namespace Application.SalesFollowup.Queries.Admin.GetEmployeeDetail;

public class GetSalesFollowupEmployeeDetailHandler
    : IRequestHandler<GetSalesFollowupEmployeeDetailQuery, Result<SalesFollowupDetailDto>>
{
    private readonly SalesFollowupDataService _data;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetSalesFollowupEmployeeDetailHandler(
        SalesFollowupDataService data,
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _data = data;
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SalesFollowupDetailDto>> Handle(
        GetSalesFollowupEmployeeDetailQuery request,
        CancellationToken cancellationToken)
    {
        var branchError = SalesFollowupAccess.ValidateBranch(_currentUser, request.BranchId);
        if (branchError is not null)
            return branchError;

        var guard = await SalesFollowupEmployeeGuard.EnsureAssignedInBranchAsync(
            _db, request.EmployeeId, request.BranchId, _clock.UtcNowOffset, cancellationToken);
        if (guard is not null)
            return guard;

        var detail = await _data.GetDetailAsync(request.EmployeeId, request.BranchId, cancellationToken);
        if (detail is null)
            return SalesFollowupErrors.EmployeeNotFound;

        return detail;
    }
}
