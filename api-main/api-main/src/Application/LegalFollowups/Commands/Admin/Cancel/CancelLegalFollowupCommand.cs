using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Commands.Admin.Cancel;

public sealed class CancelLegalFollowupCommand : IRequest<Result<CancelLegalFollowupResponse>>
{
    public int CaseId { get; init; }
    public string Reason { get; init; } = "";
    public int? ExpectedVersion { get; init; }
}

public sealed record CancelLegalFollowupResponse(
    int CaseId,
    string Status,
    string StatusLabel,
    int RowVersion,
    bool SalesRequestReturnedToUnassigned,
    int? SalesRequestId);

public sealed class CancelLegalFollowupHandler
    : IRequestHandler<CancelLegalFollowupCommand, Result<CancelLegalFollowupResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activity;

    public CancelLegalFollowupHandler(
        IApplicationDbContext db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IActivityLogService activity)
    {
        _db = db;
        _current = current;
        _clock = clock;
        _activity = activity;
    }

    public async Task<Result<CancelLegalFollowupResponse>> Handle(
        CancelLegalFollowupCommand request,
        CancellationToken cancellationToken)
    {
        if (_current.HasPermission(SeedPermissions.LegalFollowup.Cancel) == false)
            return new Error("ليس لديك صلاحية إلغاء المتابعة القانونية.", "LegalFollowup.CancelDenied");

        var reason = (request.Reason ?? "").Trim();
        if (string.IsNullOrWhiteSpace(reason))
            return new Error("سبب الإلغاء مطلوب.", "LegalFollowup.ReasonRequired");
        if (request.CaseId <= 0)
            return new Error("معرّف المتابعة غير صالح.", "LegalFollowup.InvalidId");
        if (_current.UserId is not int actorId)
            return new Error("المستخدم غير معروف.", "LegalFollowup.NoActor");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = await _db.LegalFollowupCases
                .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
            if (entity is null)
                return new Error("المتابعة غير موجودة.", "LegalFollowup.NotFound");

            var branchId = await ResolveBranchIdAsync(entity, cancellationToken);
            if (branchId is null || SalesRequestAccess.CanAccessBranch(_current, branchId.Value) == false)
                return FollowerWorkErrors.BranchAccessDenied;

            if (LegalFollowupCase.CanCancel(entity.Status) == false)
            {
                if (entity.Status == LegalFollowupStatus.Cancelled)
                {
                    await tx.CommitAsync(cancellationToken);
                    return new CancelLegalFollowupResponse(
                        entity.Id,
                        entity.Status.ToString(),
                        LegalFollowupStatusLabels.Arabic(entity.Status),
                        entity.RowVersion,
                        false,
                        entity.SalesRequestId);
                }

                return new Error("يمكن إلغاء المتابعات قيد العمل فقط.", "LegalFollowup.NotCancellable");
            }

            var beforeStatus = entity.Status;
            var beforeVersion = entity.RowVersion;
            try
            {
                entity.Cancel(reason, _clock.UtcNowOffset, request.ExpectedVersion);
            }
            catch (InvalidOperationException ex)
            {
                return new Error(ex.Message, "LegalFollowup.CancelFailed");
            }

            var requestReturned = false;
            if (entity.SalesRequestId is int srId and > 0)
            {
                var sr = await _db.SalesRequests
                    .FirstOrDefaultAsync(x => x.Id == srId, cancellationToken);
                if (sr is not null)
                {
                    var prev = sr.Status;
                    requestReturned = sr.TryReturnFromLegalFollowupCancel(
                        $"إلغاء إرسال قانونية (قضية {entity.Id}): {reason}",
                        _clock.UtcNowOffset);
                    if (requestReturned)
                    {
                        SalesRequestTimelineWriter.Append(
                            _db,
                            sr.Id,
                            SalesRequestTimelineEventType.LegalFollowupCancelled,
                            actorId,
                            _clock.UtcNowOffset,
                            prev,
                            sr.Status,
                            null,
                            null,
                            reason,
                            Guid.NewGuid());
                        SalesRequestChangeOutboxWriter.Append(
                            _db,
                            sr,
                            SalesRequestChangeType.StatusChanged,
                            _clock.UtcNowOffset);
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            await _activity.AddAsync(
                branchId.Value,
                ActivityType.CustomerUpdated,
                TargetEntityType.Customer,
                entity.CustomerId ?? entity.Id,
                details: $"CancelLegalFollowup case={entity.Id} {beforeStatus}/v{beforeVersion}→Cancelled/v{entity.RowVersion}: {reason}");
            await tx.CommitAsync(cancellationToken);

            return new CancelLegalFollowupResponse(
                entity.Id,
                entity.Status.ToString(),
                LegalFollowupStatusLabels.Arabic(entity.Status),
                entity.RowVersion,
                requestReturned,
                entity.SalesRequestId);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<int?> ResolveBranchIdAsync(LegalFollowupCase entity, CancellationToken ct)
    {
        if (entity.SalesRequestId is int srId)
        {
            return await _db.SalesRequests.AsNoTracking()
                .Where(x => x.Id == srId)
                .Select(x => (int?)x.BranchId)
                .FirstOrDefaultAsync(ct);
        }

        return await _db.OrderLists.AsNoTracking()
            .Where(l => l.MotabaId == entity.FollowerUserId)
            .Select(l => (int?)l.BranchId)
            .FirstOrDefaultAsync(ct);
    }
}
