using Application.Common.Balances;
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
        var canSeeAll = WaiverScope.CanSeeAllBranches(_current);

        var query = _db.CustomerWaiverRequests.AsNoTracking().AsQueryable();
        if (canSeeAll == false)
            query = query.Where(x => branchIds.Contains(x.BranchId));
        if (request.MineOnly && _current.UserId is int uid)
            query = query.Where(x => x.RequesterUserId == uid);
        if (request.BranchId is > 0)
            query = query.Where(x => x.BranchId == request.BranchId);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            var customerIds = _db.Customers
                .Where(c => c.FullName.Contains(s) || c.PhoneNumber.Contains(s))
                .Select(c => c.Id);
            query = query.Where(x => customerIds.Contains(x.CustomerId) || x.RequestNote.Contains(s));
        }

        var grouped = await query
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        int Of(WaiverRequestStatus status) => grouped.Where(g => g.Status == status).Sum(g => g.Count);
        var counts = new WaiverStatusCountsDto(
            Of(WaiverRequestStatus.Pending),
            Of(WaiverRequestStatus.AwaitingAccountant),
            Of(WaiverRequestStatus.Approved),
            Of(WaiverRequestStatus.Rejected));

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<WaiverRequestStatus>(request.Status, true, out var st))
            query = query.Where(x => x.Status == st);

        var total = await query.CountAsync(ct);

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

        return new WaiverListResponse(items, total, page, size, counts.Pending, counts.AwaitingAccountant, counts);
    }
}

public sealed class GetWaiverByIdHandler : IRequestHandler<GetWaiverByIdQuery, Result<WaiverDetailDto>>
{
    private const int RelatedOrdersLimit = 50;
    private const int RelatedCustomersLimit = 20;

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

        if (WaiverScope.InScope(_current, entity.BranchId) == false)
            return WaiverErrors.BranchDenied;

        var customer = await _db.Customers.AsNoTracking()
            .Where(c => c.Id == entity.CustomerId)
            .Select(c => new { c.FullName, c.PhoneNumber, c.WhatsAppPhoneNumber })
            .FirstOrDefaultAsync(ct);
        var branchName = await _db.Branches.AsNoTracking()
            .Where(b => b.Id == entity.BranchId).Select(b => b.Name).FirstOrDefaultAsync(ct);

        double? sell = null, paid = null;
        if (entity.OrderId is int oid)
        {
            sell = await _db.Orders.AsNoTracking().Where(o => o.Id == oid).Select(o => (double?)o.SellAmount).FirstOrDefaultAsync(ct);
            paid = await _db.InstallmentPayments.AsNoTracking().Where(p => p.OrderId == oid).SumAsync(p => (double?)p.Amount, ct) ?? 0;
        }

        var balances = await CustomerBalanceQuery.ForCustomersAsync(_db, [entity.CustomerId], ct);
        var own = balances.GetValueOrDefault(entity.CustomerId)
                  ?? new CustomerBalance(entity.CustomerId, 0, 0);
        var account = new WaiverAccountDto(own.SellTotal, own.PaidTotal, own.Raw, own.Rounded);

        var relatedOrders = await LoadOrdersAsync(entity.CustomerId, entity.OrderId, ct);
        var relatedCustomers = await LoadMatchesAsync(
            entity.CustomerId, entity.BranchId, customer?.FullName, customer?.PhoneNumber, customer?.WhatsAppPhoneNumber, ct);

        return WaiverMapper.ToDetail(
            entity,
            customer?.FullName ?? $"#{entity.CustomerId}",
            customer?.PhoneNumber,
            branchName,
            sell,
            paid,
            sell is null ? null : sell - (paid ?? 0)) with
        {
            Account = account,
            RelatedOrders = relatedOrders,
            RelatedCustomers = relatedCustomers
        };
    }

    private async Task<IReadOnlyList<WaiverRelatedOrderDto>> LoadOrdersAsync(int customerId, int? waiverOrderId, CancellationToken ct)
    {
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.SaleDate).ThenByDescending(o => o.Id)
            .Take(RelatedOrdersLimit)
            .Select(o => new { o.Id, o.SaleDate, o.SellAmount, o.ApprovalStatus })
            .ToListAsync(ct);
        if (orders.Count == 0)
            return [];

        var ids = orders.Select(o => o.Id).ToList();
        var paid = await _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId != null && ids.Contains(p.OrderId.Value))
            .GroupBy(p => p.OrderId!.Value)
            .Select(g => new { OrderId = g.Key, Paid = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.OrderId, x => x.Paid, ct);

        return orders.Select(o =>
        {
            var p = paid.GetValueOrDefault(o.Id);
            return new WaiverRelatedOrderDto(
                o.Id,
                o.SaleDate,
                o.SellAmount,
                p,
                o.SellAmount - p,
                o.ApprovalStatus.ToString(),
                waiverOrderId == o.Id);
        }).ToList();
    }

    private async Task<IReadOnlyList<WaiverRelatedCustomerDto>> LoadMatchesAsync(
        int customerId,
        int branchId,
        string? fullName,
        string? phone,
        string? whatsApp,
        CancellationToken ct)
    {
        var name = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();
        var phones = new[] { phone?.Trim(), whatsApp?.Trim() }
            .Where(p => string.IsNullOrEmpty(p) == false)
            .Select(p => p!)
            .Distinct()
            .ToList();
        if (name is null && phones.Count == 0)
            return [];

        var matches = await _db.Customers.AsNoTracking()
            .Where(c => c.Id != customerId && c.BranchId == branchId)
            .Where(c => (name != null && c.FullName == name)
                        || phones.Contains(c.PhoneNumber)
                        || phones.Contains(c.WhatsAppPhoneNumber))
            .OrderBy(c => c.Id)
            .Take(RelatedCustomersLimit)
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber, c.WhatsAppPhoneNumber })
            .ToListAsync(ct);
        if (matches.Count == 0)
            return [];

        var balances = await CustomerBalanceQuery.ForCustomersAsync(_db, matches.Select(m => m.Id).ToList(), ct);
        return matches.Select(m =>
        {
            var reasons = new List<string>();
            if (phones.Contains(m.PhoneNumber) || phones.Contains(m.WhatsAppPhoneNumber))
                reasons.Add("نفس رقم الهاتف");
            if (name is not null && m.FullName == name)
                reasons.Add("نفس الاسم");
            var b = balances.GetValueOrDefault(m.Id) ?? new CustomerBalance(m.Id, 0, 0);
            return new WaiverRelatedCustomerDto(m.Id, m.FullName, m.PhoneNumber, reasons, b.SellTotal, b.PaidTotal, b.Rounded);
        }).ToList();
    }
}
