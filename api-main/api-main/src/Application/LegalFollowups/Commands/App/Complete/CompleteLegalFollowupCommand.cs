using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.LegalFollowups.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Commands.App.Complete;

public sealed class CompleteLegalFollowupCommand : IRequest<Result<LegalFollowupCaseRowDto>>
{
    public int CaseId { get; init; }
    public string? Note { get; init; }
    public int? ExpectedVersion { get; init; }
}

public sealed class CompleteLegalFollowupHandler
    : IRequestHandler<CompleteLegalFollowupCommand, Result<LegalFollowupCaseRowDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activity;
    private readonly LegalFollowupDataService _data;

    public CompleteLegalFollowupHandler(
        IApplicationDbContext db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IActivityLogService activity,
        LegalFollowupDataService data)
    {
        _db = db;
        _current = current;
        _clock = clock;
        _activity = activity;
        _data = data;
    }

    public async Task<Result<LegalFollowupCaseRowDto>> Handle(
        CompleteLegalFollowupCommand request,
        CancellationToken cancellationToken)
    {
        if (_current.HasPermission(SeedPermissions.LegalFollowup.FieldComplete) == false)
            return new Error("غير مخول بإكمال المتابعة القانونية.", "LegalFollowup.FieldCompleteDenied");
        if (_current.UserId is not int followerId)
            return new Error("المستخدم غير معروف.", "LegalFollowup.NoActor");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = await _db.LegalFollowupCases
                .FirstOrDefaultAsync(c => c.Id == request.CaseId, cancellationToken);
            if (entity is null || entity.FollowerUserId != followerId)
                return new Error("المتابعة غير موجودة.", "LegalFollowup.NotFound");
            if (entity.Status != LegalFollowupStatus.InProgress)
            {
                if (entity.Status == LegalFollowupStatus.Completed)
                {
                    await tx.CommitAsync(cancellationToken);
                    var branch = await _data.ResolveBranchIdAsync(entity, cancellationToken);
                    return ToRow(entity, branch);
                }

                return new Error("المتابعة ليست قيد العمل.", "LegalFollowup.NotInProgress");
            }

            var docs = await _db.LegalFollowupCaseDocuments.AsNoTracking()
                .Where(d => d.CaseId == entity.Id)
                .Select(d => d.DocumentType)
                .ToListAsync(cancellationToken);
            var gate = LegalFollowupCompletionRules.ValidateReady(
                entity.ShopLatitude, entity.ShopLongitude, docs);
            if (gate is not null)
                return new Error(gate, "LegalFollowup.NotReady");

            try
            {
                entity.Complete(request.Note, _clock.UtcNowOffset, request.ExpectedVersion);
            }
            catch (InvalidOperationException ex)
            {
                return new Error(ex.Message, "LegalFollowup.CompleteFailed");
            }

            if (entity.SalesRequestId is int srId)
            {
                var sr = await _db.SalesRequests.FirstOrDefaultAsync(x => x.Id == srId, cancellationToken);
                if (sr is not null && sr.Status == SalesRequestStatus.LegalFollowup)
                {
                    // الإكمال الميداني لا يعيد الطلب لغير مسند ولا يحذف زبونًا —
                    // يبقى LegalFollowup حتى تُغلق دورة البيع إداريًا إن لزم.
                    SalesRequestChangeOutboxWriter.Append(
                        _db, sr, SalesRequestChangeType.StatusChanged, _clock.UtcNowOffset);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            var branchId = await _data.ResolveBranchIdAsync(entity, cancellationToken) ?? 0;
            await _activity.AddAsync(
                branchId,
                ActivityType.CustomerUpdated,
                TargetEntityType.Customer,
                entity.CustomerId ?? entity.Id,
                details: $"CompleteLegalFollowup case={entity.Id}");
            await tx.CommitAsync(cancellationToken);
            return ToRow(entity, branchId == 0 ? null : branchId);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static LegalFollowupCaseRowDto ToRow(
        Domain.Entities.LegalFollowupAggregate.LegalFollowupCase entity,
        int? branchId) => new()
    {
        Id = entity.Id,
        SalesRequestId = entity.SalesRequestId,
        CustomerId = entity.CustomerId,
        CustomerName = entity.CustomerName,
        CustomerPhone = entity.CustomerPhone,
        CustomerAddress = entity.CustomerAddress,
        FollowerUserId = entity.FollowerUserId,
        FollowerName = entity.FollowerName,
        Status = entity.Status.ToString(),
        StatusLabel = LegalFollowupStatusLabels.Arabic(entity.Status),
        Note = entity.Note,
        CompletionNote = entity.CompletionNote,
        CreatedAtUtc = entity.CreatedAtUtc,
        CompletedAtUtc = entity.CompletedAtUtc,
        RowVersion = entity.RowVersion,
        CanCancel = false,
        BranchId = branchId,
        ShopLatitude = entity.ShopLatitude,
        ShopLongitude = entity.ShopLongitude,
    };
}
