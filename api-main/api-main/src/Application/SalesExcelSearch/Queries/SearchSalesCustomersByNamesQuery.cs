using Application.Common.Interfaces;
using Application.SalesExcelSearch.Common;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesExcelSearch.Queries;

/// <summary>
/// بحث أسماء من ملف Excel لمدير المبيعات — مصدر: sales-manager-excel-search + SalesExcelCustomerSearchService.
/// نطاق الفروع من الجلسة؛ لا يعتمد على cityValue من العميل كمصدر ثقة.
/// تحميل زبائن النطاق مرة واحدة ثم مطابقة في الذاكرة — بلا N+1 لكل اسم.
/// </summary>
public sealed class SearchSalesCustomersByNamesQuery : IRequest<Result<SalesExcelSearchResponse>>
{
    public required IReadOnlyList<string> Names { get; init; }
}

public sealed class SearchSalesCustomersByNamesHandler
    : IRequestHandler<SearchSalesCustomersByNamesQuery, Result<SalesExcelSearchResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public SearchSalesCustomersByNamesHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<SalesExcelSearchResponse>> Handle(
        SearchSalesCustomersByNamesQuery request,
        CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Customer.Read) == false)
            return SalesExcelSearchErrors.Forbidden;

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0)
            return SalesExcelSearchErrors.BranchScope;

        var names = request.Names ?? [];
        if (names.Count > SalesCustomerNameMatch.MaxNames)
            return SalesExcelSearchErrors.TooManyNames;

        var customers = await _db.Customers.AsNoTracking()
            .Where(c => scope.Contains(c.BranchId))
            .Select(c => new CustomerHit(c.Id, c.FullName, c.PhoneNumber, c.BranchId))
            .ToListAsync(ct);

        var branchNames = await _db.Branches.AsNoTracking()
            .Where(b => scope.Contains(b.Id))
            .Select(b => new { b.Id, b.Name })
            .ToDictionaryAsync(b => b.Id, b => b.Name, ct);

        var queries = new List<SalesExcelSearchQueryDto>(names.Count);
        var matchedIds = new HashSet<int>();
        var pending = new List<(int RowNumber, List<CustomerHit> Hits)>();

        for (var i = 0; i < names.Count; i++)
        {
            var trimmed = (names[i] ?? "").Trim();
            var rowNumber = i + 2;
            if (trimmed.Length == 0)
            {
                queries.Add(EmptyQuery(rowNumber, trimmed, "الاسم فارغ"));
                continue;
            }

            var normalized = SalesArabicText.Normalize(trimmed);
            if (normalized.Length < SalesCustomerNameMatch.MinimumLength)
            {
                queries.Add(EmptyQuery(rowNumber, trimmed, "الاسم قصير جداً للبحث الآمن"));
                continue;
            }

            var usedFamily = SalesCustomerNameMatch.UsesFamilySearch(normalized);
            var searchKey = SalesCustomerNameMatch.FamilySearchKey(normalized);
            var hits = customers
                .Where(c => SalesCustomerNameMatch.IsMatch(c.FullName, normalized))
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            var truncated = false;
            string? warning = null;
            if (hits.Count > SalesCustomerNameMatch.MaxMatchesPerName)
            {
                truncated = true;
                warning = "الاسم عاماً جداً وأعاد عدداً كبيراً من النتائج. عُرضت أول "
                          + SalesCustomerNameMatch.MaxMatchesPerName + " نتيجة.";
                hits = hits.Take(SalesCustomerNameMatch.MaxMatchesPerName).ToList();
            }

            foreach (var h in hits)
                matchedIds.Add(h.Id);

            queries.Add(new SalesExcelSearchQueryDto(
                rowNumber, trimmed, searchKey, usedFamily, hits.Count > 0, hits.Count, truncated, warning, []));
            if (hits.Count > 0)
                pending.Add((rowNumber, hits));
        }

        var orderFacts = matchedIds.Count == 0
            ? new List<OrderFact>()
            : await _db.Orders.AsNoTracking()
                .Where(o => matchedIds.Contains(o.CustomerId) && scope.Contains(o.BranchId))
                .Select(o => new OrderFact(
                    o.Id,
                    o.CustomerId,
                    o.SellAmount,
                    o.SaleDate,
                    o.CreatedAt,
                    o.InstallmentPayments.Sum(p => p.Amount),
                    o.OrderList != null ? o.OrderList.MandobId : null,
                    o.OrderList != null && o.OrderList.Mandob != null
                        ? o.OrderList.Mandob.FullName
                        : null))
                .ToListAsync(ct);

        var salesByCustomer = orderFacts
            .GroupBy(o => o.CustomerId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(o => o.SaleDate)
                    .ThenByDescending(o => o.CreatedAt)
                    .ThenByDescending(o => o.Id)
                    .ToList());

        var mandobByCustomer = orderFacts
            .GroupBy(o => o.CustomerId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => (x.MandobId, x.MandobName))
                    .FirstOrDefault(x => x.MandobId != null || string.IsNullOrWhiteSpace(x.MandobName) == false));

        var byRow = pending.ToDictionary(p => p.RowNumber, p => p.Hits);
        var filled = new List<SalesExcelSearchQueryDto>(queries.Count);
        foreach (var q in queries)
        {
            if (byRow.TryGetValue(q.RowNumber, out var hits) == false)
            {
                filled.Add(q);
                continue;
            }

            var matches = new List<SalesExcelSearchMatchDto>(hits.Count);
            foreach (var c in hits)
            {
                branchNames.TryGetValue(c.BranchId, out var bName);
                mandobByCustomer.TryGetValue(c.Id, out var mandob);
                var sales = salesByCustomer.GetValueOrDefault(c.Id) ?? [];
                var sell = sales.Sum(s => s.SellAmount);
                var paid = sales.Sum(s => s.Paid);
                var remaining = Math.Max(0, sell - paid);
                matches.Add(new SalesExcelSearchMatchDto(
                    $"{c.BranchId}:{c.Id}",
                    c.BranchId,
                    bName,
                    c.Id,
                    c.FullName,
                    c.Phone,
                    null,
                    mandob.MandobName,
                    mandob.MandobId,
                    sell,
                    paid,
                    remaining,
                    sales.Select(s => new SalesExcelSearchSaleDto(
                        s.Id,
                        s.SaleDate?.ToDateTime(TimeOnly.MinValue),
                        s.SellAmount,
                        paid,
                        remaining,
                        remaining <= 0.0001)).ToList()));
            }

            filled.Add(q with { Matches = matches });
        }

        return new SalesExcelSearchResponse(
            true,
            filled.Count,
            filled.Count(x => x.Found),
            filled.Count(x => x.Found == false),
            filled);
    }

    private static SalesExcelSearchQueryDto EmptyQuery(int row, string name, string warning) =>
        new(row, name, "", false, false, 0, false, warning, []);

    private sealed record CustomerHit(int Id, string FullName, string? Phone, int BranchId);

    private sealed record OrderFact(
        int Id,
        int CustomerId,
        double SellAmount,
        DateOnly? SaleDate,
        DateTimeOffset? CreatedAt,
        double Paid,
        int? MandobId,
        string? MandobName);
}
