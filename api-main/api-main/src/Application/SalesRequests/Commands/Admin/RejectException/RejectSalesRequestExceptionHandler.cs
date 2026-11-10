using Application.Common.Interfaces;
using Application.SalesRequests.Common;

namespace Application.SalesRequests.Commands.Admin.RejectException;

public class RejectSalesRequestExceptionHandler : IRequestHandler<RejectSalesRequestExceptionCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RejectSalesRequestExceptionHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public Task<Result> Handle(RejectSalesRequestExceptionCommand request, CancellationToken cancellationToken)
        => SalesRequestExceptionDecider.DecideAsync(
            _db, _currentUser, _clock, request.RequestId, request.Reason, approve: false, cancellationToken);
}
