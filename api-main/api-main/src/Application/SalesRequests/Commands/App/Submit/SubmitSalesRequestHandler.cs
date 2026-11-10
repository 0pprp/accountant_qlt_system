using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.App.Submit;

public class SubmitSalesRequestHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock, ISalesRequestSubmissionLock submissionLock)
    : IRequestHandler<SubmitSalesRequestCommand, Result<SubmissionReceipt>>
{
    public async Task<Result<SubmissionReceipt>> Handle(SubmitSalesRequestCommand request, CancellationToken ct)
    {
        var permission = SalesRequestSubmissionRules.Permission(request.Source);
        if (currentUser.UserId is not int userId || permission is null || currentUser.HasPermission(permission) == false)
            return SalesRequestErrors.TabForbidden;
        if (await db.Users.AnyAsync(u => u.Id == userId && u.UserBranches.Any(b => b.BranchId == request.BranchId), ct) == false)
            return SalesRequestErrors.BranchForbidden;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await submissionLock.AcquireAsync(userId, request.SubmissionKey, ct);
        var fingerprint = SalesRequestSubmissionRules.Fingerprint(request);
        var previous = await db.SalesRequests.AsNoTracking().FirstOrDefaultAsync(
            x => x.CreatedByUserId == userId && x.SubmissionKey == request.SubmissionKey, ct);
        if (previous is not null) return Receipt(previous, fingerprint);

        var entity = new SalesRequest(request.CustomerName.Trim(), SalesRequestSubmissionRules.NormalizePhone(request.CustomerPhone),
            request.Source, request.BranchId, null, null, userId);
        var now = clock.UtcNowOffset;
        entity.SetSubmission(request.Address.Trim(), request.RequestedProduct.Trim(),
            string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(), request.SubmissionKey, fingerprint, now);

        db.SalesRequests.Add(entity);
        await db.SaveChangesAsync(ct);
        SalesRequestTimelineWriter.Append(db, entity.Id, SalesRequestTimelineEventType.Created,
            userId, now, null, SalesRequestStatus.New, null, null, null, request.SubmissionKey);
        SalesRequestChangeOutboxWriter.Append(db, entity, SalesRequestChangeType.Invalidation, now);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Receipt(entity, fingerprint);
    }

    private static Result<SubmissionReceipt> Receipt(SalesRequest entity, string fingerprint)
    {
        if (entity.SubmissionFingerprint != fingerprint) return SalesRequestErrors.ConcurrencyConflict;
        return new SubmissionReceipt(entity.Id, "submitted", entity.CreatedAt);
    }
}

public class GetSubmissionBranchesHandler(IApplicationDbContext db, ICurrentUserService user)
    : IRequestHandler<GetSubmissionBranchesQuery, Result<List<SubmissionBranch>>>
{
    public async Task<Result<List<SubmissionBranch>>> Handle(GetSubmissionBranchesQuery request, CancellationToken ct)
    {
        var permission = SalesRequestSubmissionRules.Permission(request.Source);
        if (user.UserId is not int userId || permission is null || user.HasPermission(permission) == false)
            return SalesRequestErrors.TabForbidden;
        return await db.Branches.AsNoTracking()
            .Where(b => db.Users.Any(u => u.Id == userId && u.UserBranches.Any(ub => ub.BranchId == b.Id)))
            .OrderBy(b => b.Name).Select(b => new SubmissionBranch(b.Id, b.Name)).ToListAsync(ct);
    }
}
