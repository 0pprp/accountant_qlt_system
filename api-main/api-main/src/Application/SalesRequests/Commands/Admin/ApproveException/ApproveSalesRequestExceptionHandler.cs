using Application.Common.Interfaces;
using Application.SalesRequests.Common;

namespace Application.SalesRequests.Commands.Admin.ApproveException;

public class ApproveSalesRequestExceptionHandler : IRequestHandler<ApproveSalesRequestExceptionCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public ApproveSalesRequestExceptionHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public Task<Result> Handle(ApproveSalesRequestExceptionCommand request, CancellationToken cancellationToken)
        => SalesRequestExceptionDecider.DecideAsync(
            _db, _currentUser, _clock, request.RequestId, request.Reason, approve: true, cancellationToken);
}
