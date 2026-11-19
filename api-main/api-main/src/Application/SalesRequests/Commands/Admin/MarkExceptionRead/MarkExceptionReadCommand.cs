using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Commands.Admin.MarkExceptionRead;

public record MarkExceptionReadCommand(int RequestId) : IRequest<Result<MarkExceptionReadResponse>>;

public sealed record MarkExceptionReadResponse(int Id, DateTimeOffset? ReadAtUtc, bool Changed);

public class MarkExceptionReadHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    : IRequestHandler<MarkExceptionReadCommand, Result<MarkExceptionReadResponse>>
{
    public async Task<Result<MarkExceptionReadResponse>> Handle(MarkExceptionReadCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.HasPermission(SeedPermissions.SalesRequest.TabExceptions) == false
            && currentUser.HasPermission(SeedPermissions.SalesRequest.ExceptionDecide) == false)
            return SalesRequestErrors.ActionForbidden;

        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        var entity = await db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && x.IsDeleted == false, cancellationToken);
        if (entity is null || branches.Contains(entity.BranchId) == false || entity.ExceptionHoldStatus is null)
            return SalesRequestErrors.NotFound;

        var changed = entity.MarkExceptionRead(clock.UtcNowOffset);
        if (changed)
            await db.SaveChangesAsync(cancellationToken);

        return new MarkExceptionReadResponse(entity.Id, entity.ExceptionReadAtUtc, changed);
    }
}
