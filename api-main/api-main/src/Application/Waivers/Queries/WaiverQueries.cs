using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.Waivers.Common;
using Domain.Entities.WaiverAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.Waivers.Queries;

public sealed class ListWaiversQuery : IRequest<Result<WaiverListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Status { get; init; }
    public int? BranchId { get; init; }
    public string? Search { get; init; }
    /// <summary>صندوق الفرع: طلباتي فقط.</summary>
    public bool MineOnly { get; init; }
}

public sealed class GetWaiverByIdQuery : IRequest<Result<WaiverDetailDto>>
{
    public int Id { get; init; }
}

public sealed class ListWaiversHandler : IRequestHandler<ListWaiversQuery, Result<WaiverListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListWaiversHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<WaiverListResponse>> Handle(ListWaiversQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Waiver.Read) == false)
            return WaiverErrors.Forbidden;

        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
        var branchIds = _current.BranchIds ?? [];
        var canSeeAll = _current.HasPermission(SeedPermissions.Waiver.Decide)
            || _current.HasPermission(SeedPermissions.Waiver.Apply)
            || branchIds.Count == 0;

        var query = _db.CustomerWaiverRequests.AsNoTracking().AsQueryable();
        if (canSeeAll == false)
            query = query.Where(x => branchIds.Contains(x.BranchId));
        if (request.MineOnly && _current.UserId is int uid)
            query = query.Where(x => x.RequesterUserId == uid);
        if (request.BranchId is > 0)
            query = query.Where(x => x.BranchId == request.BranchId);
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<WaiverRequestStatus>(request.Status, true, out var st))
            query = query.Where(x => x.Status == st);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            var customerIds = await _db.Customers.AsNoTracking()
                .Where(c => c.FullName.Contains(s) || c.PhoneNumber.Contains(s))
                .Select(c => c.Id)
                .ToListAsync(ct);
            query = query.Where(x => customerIds.Contains(x.CustomerId) || x.RequestNote.Contains(s));
        }

        var total = await query.CountAsync(ct);
        var pending = await query.CountAsync(x => x.Status == WaiverRequestStatus.Pending, ct);
        var awaiting = await query.CountAsync(x => x.Status == WaiverRequestStatus.AwaitingAccountant, ct);

        var rows = await query
            .OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(ct);

        var custIds = rows.Select(r => r.CustomerId).Distinct().ToList();
        var customers = await _db.Customers.AsNoTracking()
            .Where(c => custIds.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber })
            .ToDictionaryAsync(c => c.Id, ct);
        var bIds = rows.Select(r => r.BranchId).Distinct().ToList();
        var branches = await _db.Branches.AsNoTracking()
            .Where(b => bIds.Contains(b.Id))
            .Select(b => new { b.Id, b.Name })
            .ToDictionaryAsync(b => b.Id, ct);

        var items = rows.Select(w =>
        {
            customers.TryGetValue(w.CustomerId, out var c);
            branches.TryGetValue(w.BranchId, out var b);
            return WaiverMapper.ToSummary(w, c?.FullName ?? $"#{w.CustomerId}", c?.PhoneNumber, b?.Name);
        }).ToList();

        return new WaiverListResponse(items, total, page, size, pending, awaiting);
    }
}

public sealed class GetWaiverByIdHandler : IRequestHandler<GetWaiverByIdQuery, Result<WaiverDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetWaiverByIdHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<WaiverDetailDto>> Handle(GetWaiverByIdQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Waiver.Read) == false)
            return WaiverErrors.Forbidden;

        var entity = await _db.CustomerWaiverRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null)
            return WaiverErrors.NotFound;

        var branchIds = _current.BranchIds ?? [];
        var canSeeAll = _current.HasPermission(SeedPermissions.Waiver.Decide)
            || _current.HasPermission(SeedPermissions.Waiver.Apply)
            || branchIds.Count == 0;
        if (canSeeAll == false && branchIds.Contains(entity.BranchId) == false)
            return WaiverErrors.BranchDenied;

        var customer = await _db.Customers.AsNoTracking()
            .Where(c => c.Id == entity.CustomerId)
            .Select(c => new { c.FullName, c.PhoneNumber })
            .FirstOrDefaultAsync(ct);
        var branchName = await _db.Branches.AsNoTracking()
            .Where(b => b.Id == entity.BranchId).Select(b => b.Name).FirstOrDefaultAsync(ct);

        double? sell = null, paid = null;
        if (entity.OrderId is int oid)
        {
            sell = await _db.Orders.AsNoTracking().Where(o => o.Id == oid).Select(o => (double?)o.SellAmount).FirstOrDefaultAsync(ct);
            paid = await _db.InstallmentPayments.AsNoTracking().Where(p => p.OrderId == oid).SumAsync(p => (double?)p.Amount, ct) ?? 0;
        }

        return WaiverMapper.ToDetail(
            entity,
            customer?.FullName ?? $"#{entity.CustomerId}",
            customer?.PhoneNumber,
            branchName,
            sell,
            paid,
            sell is null ? null : sell - (paid ?? 0));
    }
}
