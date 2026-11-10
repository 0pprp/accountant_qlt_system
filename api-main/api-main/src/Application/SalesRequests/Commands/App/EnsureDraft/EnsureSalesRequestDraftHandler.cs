using System.Data;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Application.SalesRequests.Queries.App.GetByTab;
using Domain.Entities.OrderAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.SalesRequests.Commands.App.EnsureDraft;

public class EnsureSalesRequestDraftHandler
    : IRequestHandler<EnsureSalesRequestDraftCommand, Result<SalesRequestAppItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public EnsureSalesRequestDraftHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SalesRequestAppItemDto>> Handle(
        EnsureSalesRequestDraftCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return SalesRequestErrors.NotOwner;

        // PostgreSQL: معاملة + advisory lock. InMemory: بدون معاملة (لا دعم).
        IDbContextTransaction? transaction = null;
        if (IsNpgsql())
        {
            transaction = await _db.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted, cancellationToken);
        }

        try
        {
            await AcquireRowLockAsync(request.RequestId, cancellationToken);

            // قراءة بعد القفل — لا تعتمد على Reload لوحده أمام إعادة الإسناد.
            var entity = await _db.SalesRequests
                .FirstOrDefaultAsync(x => x.Id == request.RequestId, cancellationToken);
            if (entity is null || entity.IsDeleted)
                return SalesRequestErrors.NotFound;

            if (!entity.IsOwnedBy(userId.Value))
                return SalesRequestErrors.NotOwner;

            if (entity.Status is SalesRequestStatus.Completed
                or SalesRequestStatus.Rejected
                or SalesRequestStatus.LegalFollowup)
                return SalesRequestErrors.IllegalTransition;

            if (entity.LinkedOrderId is int existing && existing > 0)
            {
                if (transaction is not null)
                    await transaction.CommitAsync(cancellationToken);
                return ToDto(entity);
            }

            if (entity.ExistingCustomerId is null or <= 0)
                return SalesRequestErrors.CustomerRequiredForDraft;

            var customerOk = await _db.Customers.AsNoTracking()
                .AnyAsync(
                    c => c.Id == entity.ExistingCustomerId && c.BranchId == entity.BranchId,
                    cancellationToken);
            if (!customerOk)
                return SalesRequestErrors.CustomerRequiredForDraft;

            // إنشاء المسودة فقط بعد إعادة التحقق تحت القفل.
            var order = Order.CreateByAppUser(
                0, 0, 0, 0,
                entity.ExistingCustomerId.Value,
                entity.BranchId,
                userId.Value,
                []);
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(cancellationToken);

            var at = _clock.UtcNowOffset;
            var prev = entity.Status;
            try
            {
                entity.LinkDraftOrder(order.Id, at);
            }
            catch (InvalidOperationException)
            {
                if (transaction is not null)
                    await transaction.RollbackAsync(cancellationToken);
                else
                {
                    _db.Orders.Remove(order);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return SalesRequestErrors.IllegalTransition;
            }

            SalesRequestTimelineWriter.Append(
                _db, entity.Id, SalesRequestTimelineEventType.DraftLinked, userId.Value, at,
                prev, entity.Status, entity.TargetEmployeeId, entity.TargetEmployeeId, null, Guid.NewGuid());
            SalesRequestChangeOutboxWriter.Append(
                _db, entity, SalesRequestChangeType.DraftLinked, at);

            var save = await SalesRequestConcurrency.SaveWithConflictGuardAsync(
                ct => _db.SaveChangesAsync(ct), cancellationToken);
            if (save.IsFailed)
            {
                if (transaction is not null)
                    await transaction.RollbackAsync(cancellationToken);
                var again = await _db.SalesRequests.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.RequestId && !x.IsDeleted, cancellationToken);
                if (again?.LinkedOrderId is int linked && linked > 0 && again.IsOwnedBy(userId.Value))
                    return ToDto(again);
                return save.Error!;
            }

            if (transaction is not null)
                await transaction.CommitAsync(cancellationToken);
            return ToDto(entity);
        }
        catch
        {
            if (transaction is not null)
                await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    private bool IsNpgsql()
    {
        var provider = _db.Database.ProviderName ?? "";
        return provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// قفل ذري PostgreSQL (transaction-scoped advisory lock).
    /// </summary>
    private async Task AcquireRowLockAsync(int requestId, CancellationToken cancellationToken)
    {
        if (!IsNpgsql())
            return;

        await _db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(791901, {0})",
            [requestId],
            cancellationToken);
    }

    private static SalesRequestAppItemDto ToDto(Domain.Entities.SalesRequestAggregate.SalesRequest x) => new()
    {
        RequestId = x.Id,
        CustomerName = x.CustomerName,
        CustomerPhone = x.CustomerPhone,
        Status = x.Status,
        StatusLabel = SalesRequestStatusLabels.For(x.Status),
        TabKey = SalesRequestTabMap.DisplayTabForStatus(x.Status),
        SourceType = x.SourceType,
        SourceTypeLabel = SalesRequestSourceLabels.For(x.SourceType),
        BranchId = x.BranchId,
        TargetEmployeeId = x.TargetEmployeeId,
        LinkedOrderId = x.LinkedOrderId,
        CreatedAt = x.CreatedAt,
        StatusChangedAtUtc = x.StatusChangedAtUtc,
        ViewedAtUtc = x.ViewedAtUtc,
        LastStatusNote = x.LastStatusNote,
        IsReturned = x.Status == SalesRequestStatus.Returned
    };
}
