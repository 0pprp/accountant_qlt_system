using Application.BranchVisits.Common;
using Application.Common.Interfaces;
using Application.SalesRatings.Common;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.BranchCustomers.Queries;

/// <summary>
/// صف قائمة زبائن إدارة الفرع — حقول مصدر CustomersPage: اسم، مندوب، هاتف، بيع، متبقي، تقييم.
/// </summary>
public sealed record BranchCustomerSearchRowDto(
    int CustomerId,
    string CustomerName,
    string? PhoneNumber,
    string? MandobName,
    double TotalSellAmount,
    double TotalPaidAmount,
    double RemainingAmount,
    int? RatingScore,
    string? RatingLabel);

public sealed record BranchCustomerSearchResponse(IReadOnlyList<BranchCustomerSearchRowDto> Items);

public static class BranchCustomerErrors
{
    public static readonly Error Forbidden = new("لا صلاحية.", "BranchCustomer.Forbidden");
    public static readonly Error QueryTooShort = new("أدخل حرفين على الأقل للبحث.", "BranchCustomer.QueryTooShort");
}

/// <summary>
/// مصدر BM CustomersPage: بحث بالاسم/الهاتف/المعرّف ≥2 أحرف ضمن فروع الجلسة — ليس قائمة عامة ولا إنشاء.
/// </summary>
public sealed class SearchBranchCustomersQuery : IRequest<Result<BranchCustomerSearchResponse>>
{
    public required string Text { get; init; }
}

public sealed class SearchBranchCustomersHandler
    : IRequestHandler<SearchBranchCustomersQuery, Result<BranchCustomerSearchResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public SearchBranchCustomersHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<BranchCustomerSearchResponse>> Handle(
        SearchBranchCustomersQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.Customer.Read) == false)
            return BranchCustomerErrors.Forbidden;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: null, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        // أعمال الفرع مربوطة بفرع الزيارة المفتوحة — زيارة A لا تفتح زبائن B.
        var visitBranchId = visitResult.Data!.BranchId;

        var q = (request.Text ?? "").Trim();
        if (q.Length < 2 && int.TryParse(q, out _) == false)
            return BranchCustomerErrors.QueryTooShort;

        var idFilter = int.TryParse(q, out var cid) ? cid : (int?)null;

        var customers = await _db.Customers.AsNoTracking()
            .Where(c => c.BranchId == visitBranchId
                        && (idFilter != null && c.Id == idFilter
                            || c.FullName.Contains(q)
                            || c.PhoneNumber.Contains(q)))
            .OrderBy(c => c.FullName)
            .Take(100)
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber })
            .ToListAsync(ct);

        if (customers.Count == 0)
            return new BranchCustomerSearchResponse([]);

        var ids = customers.Select(c => c.Id).ToList();
        var orderFacts = await _db.Orders.AsNoTracking()
            .Where(o => ids.Contains(o.CustomerId) && o.BranchId == visitBranchId)
            .Select(o => new
            {
                o.CustomerId,
                o.SellAmount,
                o.SaleDate,
                o.CreatedAt,
                MandobName = o.OrderList != null && o.OrderList.Mandob != null
                    ? o.OrderList.Mandob.FullName
                    : null,
                LastPay = o.InstallmentPayments.Max(p => (DateOnly?)p.Date),
                Paid = o.InstallmentPayments.Sum(p => p.Amount)
            })
            .ToListAsync(ct);

        var asOf = DateOnly.FromDateTime(_clock.UtcNowOffset.UtcDateTime);
        var rows = customers.Select(c =>
        {
            var orders = orderFacts.Where(o => o.CustomerId == c.Id).ToList();
            var sell = orders.Sum(o => o.SellAmount);
            var paid = orders.Sum(o => o.Paid);
            var remaining = Math.Max(0, sell - paid);
            var mandob = orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => o.MandobName)
                .FirstOrDefault(n => string.IsNullOrWhiteSpace(n) == false);
            var saleDates = orders
                .Where(o => o.SaleDate != null)
                .Select(o => o.SaleDate!.Value)
                .ToList();
            DateOnly? saleOnly = saleDates.Count == 0 ? null : saleDates.Min();
            var lastPays = orders.Where(o => o.LastPay != null).Select(o => o.LastPay!.Value).ToList();
            DateOnly? lastPay = lastPays.Count == 0 ? null : lastPays.Max();
            var score = CustomerRatingCalculator.ScoreFor(sell, paid, remaining, saleOnly, lastPay, asOf);
            var label = score is int s ? CustomerRatingCalculator.LevelName(s) : null;
            return new BranchCustomerSearchRowDto(
                c.Id, c.FullName, c.PhoneNumber, mandob, sell, paid, remaining, score, label);
        }).ToList();

        return new BranchCustomerSearchResponse(rows);
    }
}
