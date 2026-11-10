using Application.Common.Interfaces;
using Application.LegalFollowups.Common;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.LegalFollowupAggregate;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.LegalFollowups.Commands.Admin.Create;

public sealed class CreateLegalFollowupCommand : IRequest<Result<LegalFollowupCaseRowDto>>
{
    public int SalesRequestId { get; init; }
    public int FollowerUserId { get; init; }
    public string? Note { get; init; }
    public int? ExpectedConcurrencyStamp { get; init; }
}

public sealed class CreateLegalFollowupHandler
    : IRequestHandler<CreateLegalFollowupCommand, Result<LegalFollowupCaseRowDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IActivityLogService _activity;

    public CreateLegalFollowupHandler(
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

    public async Task<Result<LegalFollowupCaseRowDto>> Handle(
        CreateLegalFollowupCommand request,
        CancellationToken cancellationToken)
    {
        if (_current.HasPermission(SeedPermissions.LegalFollowup.Create) == false)
            return new Error("غير مخول بإنشاء متابعة قانونية.", "LegalFollowup.CreateDenied");

        if (_current.UserId is not int actorId)
            return new Error("المستخدم غير معروف.", "LegalFollowup.NoActor");

        if (request.SalesRequestId <= 0 || request.FollowerUserId <= 0)
            return new Error("معرّفات غير صالحة.", "LegalFollowup.InvalidIds");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var sr = await _db.SalesRequests
                .FirstOrDefaultAsync(x => x.Id == request.SalesRequestId, cancellationToken);
            if (sr is null)
                return new Error("طلب البيع غير موجود.", "LegalFollowup.RequestNotFound");

            if (SalesRequestAccess.CanAccessBranch(_current, sr.BranchId) == false)
                return new Error("لا صلاحية على فرع الطلب.", "LegalFollowup.BranchForbidden");

            if (request.ExpectedConcurrencyStamp is int stamp && stamp != sr.ConcurrencyStamp)
                return new Error("تعارض تحديث الطلب — أعد التحميل.", "LegalFollowup.ConcurrencyConflict");

            if (SalesRequest.CanSendToLegalFollowup(sr) == false)
                return new Error(
                    "يُسمح بإرسال الطلبات غير المسندة أو المرفوضة فقط للمتابعة القانونية.",
                    "LegalFollowup.CannotSend");

            if (string.IsNullOrWhiteSpace(sr.CustomerName)
                || string.IsNullOrWhiteSpace(sr.CustomerPhone)
                || string.IsNullOrWhiteSpace(sr.Address))
                return new Error("اسم الزبون والهاتف والعنوان مطلوبة قبل الإرسال.", "LegalFollowup.CustomerIncomplete");

            var followerOnBranch = await _db.OrderLists.AsNoTracking()
                .AnyAsync(
                    l => l.MotabaId == request.FollowerUserId && l.BranchId == sr.BranchId,
                    cancellationToken);
            if (followerOnBranch == false)
                return new Error("المتابع غير مرتبط بفرع الطلب.", "LegalFollowup.FollowerNotInBranch");

            var followerName = await _db.Users.AsNoTracking()
                .Where(u => u.Id == request.FollowerUserId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken);

            var openExists = await _db.LegalFollowupCases.AsNoTracking()
                .AnyAsync(
                    c => c.SalesRequestId == sr.Id && c.Status == LegalFollowupStatus.InProgress,
                    cancellationToken);
            if (openExists)
                return new Error("هذا الطلب مُرسل مسبقاً وما زال قيد العمل.", "LegalFollowup.AlreadyOpen");

            var now = _clock.UtcNowOffset;
            var note = string.IsNullOrWhiteSpace(request.Note)
                ? $"إرسال للمتابع: {followerName}"
                : request.Note.Trim();

            var entity = new LegalFollowupCase(
                sr.Id,
                sr.ExistingCustomerId,
                request.FollowerUserId,
                followerName,
                sr.CustomerName.Trim(),
                sr.CustomerPhone!.Trim(),
                sr.Address!.Trim(),
                note,
                actorId,
                null,
                now);

            _db.LegalFollowupCases.Add(entity);

            var prev = sr.Status;
            var prevEmployee = sr.TargetEmployeeId;
            sr.SendToLegalFollowup(note, now);

            SalesRequestTimelineWriter.Append(
                _db,
                sr.Id,
                SalesRequestTimelineEventType.LegalFollowupSent,
                actorId,
                now,
                prev,
                sr.Status,
                null,
                null,
                note,
                Guid.NewGuid());

            SalesRequestChangeOutboxWriter.Append(
                _db, sr, SalesRequestChangeType.StatusChanged, now, prevEmployee);

            await _db.SaveChangesAsync(cancellationToken);
            await _activity.AddAsync(
                sr.BranchId,
                ActivityType.CustomerUpdated,
                TargetEntityType.Customer,
                sr.ExistingCustomerId ?? sr.Id,
                details: $"CreateLegalFollowup case={entity.Id} request={sr.Id} follower={request.FollowerUserId}");
            await tx.CommitAsync(cancellationToken);

            return new LegalFollowupCaseRowDto
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
                CanCancel = true,
                BranchId = sr.BranchId,
            };
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return new Error(ex.Message, "LegalFollowup.CreateFailed");
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
