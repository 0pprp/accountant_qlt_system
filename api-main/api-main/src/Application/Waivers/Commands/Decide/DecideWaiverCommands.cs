using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.Waivers.Common;
using Domain.Entities.CustomerDecisionAggregate;
using Domain.Entities.CustomerDecisionAggregate.Enums;
using Domain.Entities.WaiverAggregate;
using Domain.Entities.WaiverAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Waivers.Commands.Decide;

public sealed class RejectWaiverCommand : IRequest<Result<WaiverDetailDto>>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
    public string? Note { get; init; }
}

public sealed class ApproveWaiverByDelegateCommand : IRequest<Result<WaiverDetailDto>>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
    public double ApprovedAmount { get; init; }
    public string? Note { get; init; }
}

public sealed class ApplyWaiverByAccountantCommand : IRequest<Result<WaiverDetailDto>>
{
    public int Id { get; init; }
    public int ExpectedVersion { get; init; }
}

file static class WaiverDecideSupport
{
    public static async Task<Result<(Domain.Entities.WaiverAggregate.CustomerWaiverRequest Entity, string CustomerName, string? Phone, string? BranchName, double? Sell, double? Paid)>>
        LoadAsync(IApplicationDbContext db, ICurrentUserService current, int id, CancellationToken ct)
    {
        var entity = await db.CustomerWaiverRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            return WaiverErrors.NotFound;

        var branchIds = current.BranchIds ?? [];
        if (branchIds.Count > 0
            && current.HasPermission(SeedPermissions.Waiver.Apply) == false
            && current.HasPermission(SeedPermissions.Waiver.Decide) == false
            && branchIds.Contains(entity.BranchId) == false)
            return WaiverErrors.BranchDenied;

        var customer = await db.Customers.AsNoTracking()
            .Where(c => c.Id == entity.CustomerId)
            .Select(c => new { c.FullName, c.PhoneNumber })
            .FirstOrDefaultAsync(ct);
        var branchName = await db.Branches.AsNoTracking()
            .Where(b => b.Id == entity.BranchId).Select(b => b.Name).FirstOrDefaultAsync(ct);

        double? sell = null, paid = null;
        if (entity.OrderId is int oid)
        {
            sell = await db.Orders.AsNoTracking().Where(o => o.Id == oid).Select(o => (double?)o.SellAmount).FirstOrDefaultAsync(ct);
            paid = await db.InstallmentPayments.AsNoTracking().Where(p => p.OrderId == oid).SumAsync(p => (double?)p.Amount, ct) ?? 0;
        }
        else
        {
            var orderIds = await db.Orders.AsNoTracking().Where(o => o.CustomerId == entity.CustomerId).Select(o => o.Id).ToListAsync(ct);
            sell = await db.Orders.AsNoTracking().Where(o => o.CustomerId == entity.CustomerId).SumAsync(o => (double?)o.SellAmount, ct) ?? 0;
            paid = orderIds.Count == 0
                ? 0
                : await db.InstallmentPayments.AsNoTracking().Where(p => orderIds.Contains(p.OrderId)).SumAsync(p => (double?)p.Amount, ct) ?? 0;
        }

        return (entity, customer?.FullName ?? $"#{entity.CustomerId}", customer?.PhoneNumber, branchName, sell, paid);
    }
}

public sealed class RejectWaiverHandler : IRequestHandler<RejectWaiverCommand, Result<WaiverDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public RejectWaiverHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<WaiverDetailDto>> Handle(RejectWaiverCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Waiver.Decide) == false)
            return WaiverErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return WaiverErrors.Forbidden;

        var loaded = await WaiverDecideSupport.LoadAsync(_db, _current, request.Id, ct);
        if (loaded.IsFailed)
            return loaded.Error!;

        var (entity, name, phone, branch, sell, paid) = loaded.Data!;
        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        try
        {
            entity.Reject(actorId, actorName, request.Note, request.ExpectedVersion, _clock.UtcNowOffset);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("صاحب الطلب"))
        {
            return WaiverErrors.SelfDecide;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
        {
            return WaiverErrors.Concurrency;
        }
        catch (InvalidOperationException)
        {
            return WaiverErrors.InvalidTransition;
        }

        var saved = await WaiverConcurrency.SaveWithConflictGuardAsync(
            ct2 => _db.SaveChangesAsync(ct2), ct);
        if (saved.IsFailed)
            return saved.Error!;
        return WaiverMapper.ToDetail(entity, name, phone, branch, sell, paid, (sell ?? 0) - (paid ?? 0));
    }
}

public sealed class ApproveWaiverByDelegateHandler
    : IRequestHandler<ApproveWaiverByDelegateCommand, Result<WaiverDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public ApproveWaiverByDelegateHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<WaiverDetailDto>> Handle(ApproveWaiverByDelegateCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Waiver.Decide) == false)
            return WaiverErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return WaiverErrors.Forbidden;
        if (request.ApprovedAmount <= 0)
            return WaiverErrors.AmountRequired;

        var loaded = await WaiverDecideSupport.LoadAsync(_db, _current, request.Id, ct);
        if (loaded.IsFailed)
            return loaded.Error!;

        var (entity, name, phone, branch, sell, paid) = loaded.Data!;
        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        var at = _clock.UtcNowOffset;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            entity.ApproveByDelegate(actorId, actorName, request.ApprovedAmount, request.Note, request.ExpectedVersion, at);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("صاحب الطلب"))
        {
            return WaiverErrors.SelfDecide;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
        {
            return WaiverErrors.Concurrency;
        }
        catch (InvalidOperationException)
        {
            return WaiverErrors.InvalidTransition;
        }

        _db.CustomerDecisions.Add(new CustomerDecision(
            entity.CustomerId, entity.BranchId, actorId, actorName,
            CustomerDecisionType.WaiverRequest,
            $"موافقة مفوض بمبلغ {request.ApprovedAmount:0.##}. {request.Note}".Trim(),
            null, sell, null, null, entity.Id, null, at));
        var saved = await WaiverConcurrency.SaveWithConflictGuardAsync(
            ct2 => _db.SaveChangesAsync(ct2), ct);
        if (saved.IsFailed)
            return saved.Error!;
        await tx.CommitAsync(ct);
        return WaiverMapper.ToDetail(entity, name, phone, branch, sell, paid, (sell ?? 0) - (paid ?? 0));
    }
}

public sealed class ApplyWaiverByAccountantHandler
    : IRequestHandler<ApplyWaiverByAccountantCommand, Result<WaiverDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public ApplyWaiverByAccountantHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<WaiverDetailDto>> Handle(ApplyWaiverByAccountantCommand request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Waiver.Apply) == false)
            return WaiverErrors.Forbidden;
        if (_current.UserId is not int actorId)
            return WaiverErrors.Forbidden;

        var loaded = await WaiverDecideSupport.LoadAsync(_db, _current, request.Id, ct);
        if (loaded.IsFailed)
            return loaded.Error!;

        var (entity, name, phone, branch, sell, paid) = loaded.Data!;
        if (entity.Status == WaiverRequestStatus.Approved && entity.Applied)
            return WaiverMapper.ToDetail(entity, name, phone, branch, sell, paid, (sell ?? 0) - (paid ?? 0));

        if (entity.ApprovedAmount is not double requested || requested <= 0)
            return WaiverErrors.AmountRequired;

        double capped;
        try
        {
            capped = WaiverDiscount.Cap(sell ?? 0, paid ?? 0, requested);
        }
        catch (ArgumentOutOfRangeException)
        {
            return WaiverErrors.AmountRequired;
        }

        if (capped <= 0)
            return WaiverErrors.NothingToApply;

        var actorName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == actorId).Select(u => u.FullName).FirstOrDefaultAsync(ct);
        var at = _clock.UtcNowOffset;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        bool changed;
        try
        {
            changed = entity.ApplyByAccountant(actorId, actorName, capped, request.ExpectedVersion, at);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("نسخة"))
        {
            return WaiverErrors.Concurrency;
        }
        catch (InvalidOperationException)
        {
            return WaiverErrors.InvalidTransition;
        }

        if (changed)
        {
            if (entity.OrderId is int oid)
            {
                var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == oid, ct);
                if (order is not null)
                    order.SellAmount = Math.Max(0, order.SellAmount - capped);
            }
            else
            {
                // خصم من أحدث بيع للزبون عند غياب OrderId
                var order = await _db.Orders
                    .Where(o => o.CustomerId == entity.CustomerId)
                    .OrderByDescending(o => o.Id)
                    .FirstOrDefaultAsync(ct);
                if (order is not null)
                    order.SellAmount = Math.Max(0, order.SellAmount - capped);
            }

            _db.CustomerDecisions.Add(new CustomerDecision(
                entity.CustomerId, entity.BranchId, actorId, actorName,
                CustomerDecisionType.Waiver,
                $"تنازل مطبّق بمبلغ {capped:0.##}",
                null, sell, null, null, entity.Id, null, at));
            var saved = await WaiverConcurrency.SaveWithConflictGuardAsync(
                ct2 => _db.SaveChangesAsync(ct2), ct);
            if (saved.IsFailed)
                return saved.Error!;
        }

        await tx.CommitAsync(ct);
        sell = (sell ?? 0) - (changed ? capped : 0);
        return WaiverMapper.ToDetail(entity, name, phone, branch, sell, paid, (sell ?? 0) - (paid ?? 0));
    }
}
