using Application.BranchVisits.Common;
using Application.Common.Interfaces;
using Application.CustomerDecisions.Common;
using Application.SalesRequests.Common;
using Domain.Entities.CustomerDecisionAggregate.Enums;
using Domain.Entities.LegalConversionAggregate.Enums;
using Domain.Entities.WaiverAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.CustomerDecisions.Queries;

public sealed class ListLegalNamesQuery : IRequest<Result<LegalNamesResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? Search { get; init; }
}

public sealed class ListCustomerDecisionsQuery : IRequest<Result<CustomerDecisionListResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? DecisionType { get; init; }
    public int? CustomerId { get; init; }
    public string? Search { get; init; }
}

public sealed class ListLegalNamesHandler : IRequestHandler<ListLegalNamesQuery, Result<LegalNamesResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public ListLegalNamesHandler(IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<LegalNamesResponse>> Handle(ListLegalNamesQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.LegalNames.Read) == false)
            return CustomerDecisionErrors.Forbidden;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: null, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        var visitBranchId = visitResult.Data!.BranchId;

        var today = _clock.Today;
        var weekStart = today.AddDays(-7);
        var now = _clock.UtcNowOffset;

        var customers = await _db.Customers.AsNoTracking()
            .Where(c => c.BranchId == visitBranchId)
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber, c.BranchId, c.IsLegal, c.IsFakeSale })
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            customers = customers.Where(c =>
                c.FullName.Contains(s, StringComparison.OrdinalIgnoreCase)
                || (c.PhoneNumber?.Contains(s) ?? false)).ToList();
        }

        var custIds = customers.Select(c => c.Id).ToList();
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => custIds.Contains(o.CustomerId))
            .Select(o => new { o.Id, o.CustomerId, o.SellAmount, o.OrderListId })
            .ToListAsync(ct);
        var orderIds = orders.Select(o => o.Id).ToList();
        var payments = orderIds.Count == 0
            ? []
            : await _db.InstallmentPayments.AsNoTracking()
                .Where(p => orderIds.Contains(p.OrderId))
                .Select(p => new { p.OrderId, p.Amount, p.Date })
                .ToListAsync(ct);

        var pendingLegal = await _db.LegalConversionRequests.AsNoTracking()
            .Where(x => custIds.Contains(x.CustomerId) && x.Status == LegalConversionStatus.Pending)
            .Select(x => x.CustomerId).Distinct().ToListAsync(ct);
        var openWaivers = await _db.CustomerWaiverRequests.AsNoTracking()
            .Where(x => custIds.Contains(x.CustomerId)
                && (x.Status == WaiverRequestStatus.Pending || x.Status == WaiverRequestStatus.AwaitingAccountant))
            .Select(x => x.CustomerId).Distinct().ToListAsync(ct);
        var snoozes = await _db.CustomerDecisions.AsNoTracking()
            .Where(d => custIds.Contains(d.CustomerId)
                && d.DecisionType == CustomerDecisionType.Continue
                && d.SnoozeUntil != null && d.SnoozeUntil > now)
            .Select(d => d.CustomerId).Distinct().ToListAsync(ct);

        var branches = await _db.Branches.AsNoTracking()
            .Where(b => b.Id == visitBranchId)
            .Select(b => new { b.Id, b.Name })
            .ToDictionaryAsync(b => b.Id, ct);
        var listIds = orders.Where(o => o.OrderListId != null).Select(o => o.OrderListId!.Value).Distinct().ToList();
        var lists = listIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.OrderLists.AsNoTracking()
                .Where(l => listIds.Contains(l.Id))
                .Select(l => new { l.Id, l.Name })
                .ToDictionaryAsync(l => l.Id, l => l.Name, ct);

        var rows = new List<LegalNameRowDto>();
        foreach (var c in customers)
        {
            var custOrders = orders.Where(o => o.CustomerId == c.Id).ToList();
            var custOrderIds = custOrders.Select(o => o.Id).ToHashSet();
            var custPays = payments.Where(p => custOrderIds.Contains(p.OrderId)).ToList();
            var saleTotal = custOrders.Sum(o => o.SellAmount);
            var paidTotal = custPays.Sum(p => p.Amount);
            var remaining = saleTotal - paidTotal;
            var weekPaid = custPays
                .Where(p => p.Date >= weekStart)
                .Sum(p => p.Amount);
            DateOnly? lastPay = custPays.Count == 0 ? null : custPays.Max(p => p.Date);
            var days = lastPay is null ? 9999 : today.DayNumber - lastPay.Value.DayNumber;
            var pct = saleTotal <= 0 ? 0 : weekPaid * 100.0 / saleTotal;

            if (WeakPayerRules.Matches(
                    saleTotal, weekPaid, remaining, lastPay, today,
                    c.IsLegal, c.IsFakeSale,
                    snoozes.Contains(c.Id),
                    pendingLegal.Contains(c.Id),
                    openWaivers.Contains(c.Id)) == false)
                continue;

            var primaryList = custOrders.Select(o => o.OrderListId).FirstOrDefault(id => id != null);
            branches.TryGetValue(c.BranchId, out var b);
            string? listName = null;
            if (primaryList is int lid)
                lists.TryGetValue(lid, out listName!);

            rows.Add(new LegalNameRowDto(
                c.Id, c.FullName, c.PhoneNumber, c.BranchId, b?.Name,
                primaryList, listName, saleTotal, weekPaid, remaining, pct, lastPay, days));
        }

        rows = rows.OrderByDescending(r => r.DaysSinceLastPayment).ThenBy(r => r.CustomerName).ToList();
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 50 : request.PageSize;
        var total = rows.Count;
        var items = rows.Skip((page - 1) * size).Take(size).ToList();
        return new LegalNamesResponse(items, total, page, size);
    }
}

public sealed class ListCustomerDecisionsHandler
    : IRequestHandler<ListCustomerDecisionsQuery, Result<CustomerDecisionListResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ListCustomerDecisionsHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<CustomerDecisionListResponse>> Handle(
        ListCustomerDecisionsQuery request,
        CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.CustomerDecision.Read) == false)
            return CustomerDecisionErrors.Forbidden;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: null, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        var visitBranchId = visitResult.Data!.BranchId;
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize is < 1 or > 100 ? 50 : request.PageSize;

        var query = _db.CustomerDecisions.AsNoTracking()
            .Where(d => d.BranchId == visitBranchId);
        if (request.CustomerId is > 0)
            query = query.Where(d => d.CustomerId == request.CustomerId);
        if (!string.IsNullOrWhiteSpace(request.DecisionType)
            && Enum.TryParse<CustomerDecisionType>(request.DecisionType, true, out var dt))
            query = query.Where(d => d.DecisionType == dt);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            var ids = await _db.Customers.AsNoTracking()
                .Where(c => c.FullName.Contains(s) || c.PhoneNumber.Contains(s))
                .Select(c => c.Id).ToListAsync(ct);
            query = query.Where(d => ids.Contains(d.CustomerId) || d.Note.Contains(s));
        }

        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(d => d.CreatedAtUtc).ThenByDescending(d => d.Id)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(ct);

        var custIds = rows.Select(r => r.CustomerId).Distinct().ToList();
        var customers = await _db.Customers.AsNoTracking()
            .Where(c => custIds.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber, c.IsLegal, c.IsFakeSale })
            .ToDictionaryAsync(c => c.Id, ct);

        var items = rows.Select(d =>
        {
            customers.TryGetValue(d.CustomerId, out var c);
            return new CustomerDecisionRowDto(
                d.Id, d.CustomerId, c?.FullName ?? $"#{d.CustomerId}", c?.PhoneNumber,
                d.BranchId,
                d.DecisionType.ToString(),
                CustomerDecisionTypeLabels.Arabic(d.DecisionType),
                d.Note, d.ActorName, d.WeekPaid, d.AmountTotalSales, d.PaidPercent,
                d.SnoozeUntil, d.CreatedAtUtc,
                c?.IsLegal ?? false, c?.IsFakeSale ?? false);
        }).ToList();

        return new CustomerDecisionListResponse(items, total, page, size);
    }
}
