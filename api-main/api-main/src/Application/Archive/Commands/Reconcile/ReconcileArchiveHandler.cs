using Application.SalesRequests.Common;
using Application.Archive.Common;
using Application.Common.Interfaces;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Archive.Commands.Reconcile;

public class ReconcileArchiveHandler
    : IRequestHandler<ReconcileArchiveCommand, Result<ArchiveReconcileReport>>
{
    private readonly ArchiveReconciler _reconciler;
    private readonly ICurrentUserService _currentUser;

    public ReconcileArchiveHandler(ArchiveReconciler reconciler, ICurrentUserService currentUser)
    {
        _reconciler = reconciler;
        _currentUser = currentUser;
    }

    public async Task<Result<ArchiveReconcileReport>> Handle(
        ReconcileArchiveCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.Archive.Reconcile) == false)
            return ArchiveErrors.ActionForbidden;

        return await _reconciler.RunAsync(null, cancellationToken);
    }
}
