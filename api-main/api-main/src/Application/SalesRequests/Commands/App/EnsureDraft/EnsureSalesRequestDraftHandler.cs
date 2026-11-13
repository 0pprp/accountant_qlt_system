using System.Data;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Application.SalesRequests.Queries.App.GetByTab;
using Domain.Entities.CustomerAggregate;
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

            var customerId = await ResolveOrCreateCustomerAsync(entity, cancellationToken);
            if (customerId <= 0)
                return SalesRequestErrors.CustomerRequiredForDraft;

            entity.ExistingCustomerId = customerId;

            var order = Order.CreateByAppUser(
                0, 0, 0, 0,
                customerId,
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

    private async Task<int> ResolveOrCreateCustomerAsync(
        Domain.Entities.SalesRequestAggregate.SalesRequest entity,
        CancellationToken cancellationToken)
    {
        if (entity.ExistingCustomerId is int linked && linked > 0)
        {
            var ok = await _db.Customers.AsNoTracking()
                .AnyAsync(c => c.Id == linked && c.BranchId == entity.BranchId, cancellationToken);
            if (ok)
                return linked;
        }

        var phone = (entity.CustomerPhone ?? string.Empty).Trim();
        if (phone.Length > 0)
        {
            var byPhone = await _db.Customers.AsNoTracking()
                .Where(c => c.BranchId == entity.BranchId && c.PhoneNumber == phone)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (byPhone > 0)
                return byPhone;
        }

        var fullName = Clip(entity.CustomerName, 50);
        if (string.IsNullOrWhiteSpace(fullName))
            fullName = $"طلب #{entity.Id}";

        var nationalCode = Clip($"TEMP-SR-{entity.Id}", 50);
        var nationalTaken = await _db.Customers.AsNoTracking()
            .AnyAsync(c => c.NationalCode == nationalCode, cancellationToken);
        if (nationalTaken)
            nationalCode = Clip($"TEMP-SR-{entity.Id}-{Guid.NewGuid():N}", 50);

        var safePhone = Clip(string.IsNullOrWhiteSpace(phone) ? $"0{entity.Id:D10}" : phone, 30);
        var address = Clip(string.IsNullOrWhiteSpace(entity.Address) ? "—" : entity.Address!, 300);

        var customer = new Customer(
            fullName,
            "غير محدد",
            nationalCode,
            new DateOnly(1990, 1, 1),
            safePhone,
            safePhone,
            new Business(Clip(fullName, 100), address, "—"),
            entity.BranchId);

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }

    private static string Clip(string value, int max)
    {
        var v = value.Trim();
        return v.Length <= max ? v : v[..max];
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
