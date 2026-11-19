using Application.Common.Balances;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.WrongSales.Common;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.WrongSaleAggregate;
using Domain.Services.SalesMatching;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.WrongSales.Queries;

public sealed class GetWrongSaleDetailQuery : IRequest<Result<WrongSaleDetailDto>>
{
    public int Id { get; init; }
}

public static class WrongSaleDetailRules
{
    public const int MaxPayments = 20;
    public const int MaxMatches = 20;
    public const int MaxMatchCandidates = 2000;
    public const string PhoneReason = "نفس رقم الهاتف";
    public const string NameReason = "نفس الاسم الثلاثي";

    public static bool IsFromFollower(string? sourceApp)
        => string.Equals(sourceApp, WrongSaleNotice.SourceMandobFirstCollection, StringComparison.OrdinalIgnoreCase) == false;

    public static string EvaluationTitle(string? sourceApp)
        => IsFromFollower(sourceApp) ? "تقييم المتابع" : "تقييم مندوب الجباية";

    public static string EvaluatorLabel(string? sourceApp)
        => IsFromFollower(sourceApp) ? "اسم المتابع" : "اسم مندوب الجباية";

    public static decimal Money(double value)
        => decimal.Round((decimal)value, 0, MidpointRounding.AwayFromZero);

    public static IReadOnlyList<string> MatchReasons(string? queryName, string? queryPhone, string? candidateName, string? candidatePhone)
    {
        var reasons = new List<string>(2);
        if (SalesPhoneNormalizer.HasEnoughPhoneData(queryPhone) && SalesPhoneNormalizer.Matches(queryPhone, candidatePhone))
            reasons.Add(PhoneReason);
        if (SalesRequestNameSimilarity.HasEnoughTripleName(queryName)
            && SalesRequestNameSimilarity.IsTripleNameMatch(queryName, candidateName))
            reasons.Add(NameReason);
        return reasons;
    }
}

public sealed class GetWrongSaleDetailHandler : IRequestHandler<GetWrongSaleDetailQuery, Result<WrongSaleDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public GetWrongSaleDetailHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<WrongSaleDetailDto>> Handle(GetWrongSaleDetailQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.WrongSale.Read) == false)
            return WrongSaleErrors.Forbidden;

        var scope = _current.BranchIds ?? [];
        if (scope.Count == 0)
            return WrongSaleErrors.BranchScope;

        var notice = await _db.WrongSaleNotices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (notice is null)
            return WrongSaleErrors.NotFound;
        if (scope.Contains(notice.BranchId) == false)
            return WrongSaleErrors.BranchScope;

        var customerEntity = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == notice.CustomerId, ct);
        var customer = customerEntity is null
            ? null
            : new { customerEntity.FullName, customerEntity.PhoneNumber, customerEntity.BranchId, Address = customerEntity.Business?.Address };

        var order = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == notice.OrderId)
            .Select(o => new
            {
                o.SellAmount,
                o.DailyInstallmentAmount,
                o.SaleDate,
                o.OrderListId,
                ListName = o.OrderList != null ? o.OrderList.Name : null
            })
            .FirstOrDefaultAsync(ct);

        var branchName = await _db.Branches.AsNoTracking()
            .Where(b => b.Id == notice.BranchId)
            .Select(b => b.Name)
            .FirstOrDefaultAsync(ct);

        string? reviewerName = null;
        if (notice.BranchManagerReviewedByUserId is int reviewerId)
        {
            reviewerName = await _db.Users.AsNoTracking()
                .Where(u => u.Id == reviewerId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(ct);
        }

        var paidOnOrder = await _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId == notice.OrderId)
            .SumAsync(p => p.Amount, ct);

        var paymentsQuery = _db.InstallmentPayments.AsNoTracking()
            .Where(p => p.OrderId == notice.OrderId
                        || (p.OrderId == null && p.CustomerId == notice.CustomerId));
        var paymentsCount = await paymentsQuery.CountAsync(ct);
        var payments = await paymentsQuery
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.Id)
            .Take(WrongSaleDetailRules.MaxPayments)
            .Select(p => new { p.Id, p.Date, p.Amount, p.Description })
            .ToListAsync(ct);

        var balances = await CustomerBalanceQuery.ForCustomersAsync(_db, [notice.CustomerId], ct);
        var customerRemaining = balances.TryGetValue(notice.CustomerId, out var own) ? own.Rounded : 0d;

        var sell = order?.SellAmount ?? 0d;
        var customerName = string.IsNullOrWhiteSpace(customer?.FullName) ? notice.CustomerName : customer!.FullName;
        var customerPhone = string.IsNullOrWhiteSpace(customer?.PhoneNumber) ? notice.CustomerPhone : customer!.PhoneNumber;

        var matches = await LoadMatchesAsync(notice.BranchId, notice.CustomerId, customerName, customerPhone, ct);

        var dto = new WrongSaleDetailDto(
            WrongSaleMapper.ToDto(notice),
            notice.BranchManagerReviewedAtUtc is DateTimeOffset reviewedAt
                ? new WrongSaleReviewInfoDto(reviewerName, reviewedAt, notice.BranchManagerReviewNote)
                : null,
            new WrongSaleCustomerInfoDto(
                customerName,
                customerPhone,
                notice.BranchId,
                branchName,
                customer?.Address,
                order?.OrderListId,
                order?.ListName),
            new WrongSaleSaleInfoDto(
                WrongSaleDetailRules.Money(sell),
                WrongSaleDetailRules.Money(order?.DailyInstallmentAmount ?? 0d),
                WrongSaleDetailRules.Money(paidOnOrder),
                WrongSaleDetailRules.Money(CustomerBalanceRules.RoundToUnit(sell - paidOnOrder)),
                WrongSaleDetailRules.Money(customerRemaining),
                order?.SaleDate),
            new WrongSaleEvaluationDto(
                notice.SourceApp,
                WrongSaleDetailRules.EvaluationTitle(notice.SourceApp),
                WrongSaleDetailRules.EvaluatorLabel(notice.SourceApp),
                notice.FollowerName,
                order?.ListName,
                notice.Rating > 0,
                notice.Rating,
                notice.Note,
                notice.ShopStockEstimatedValue,
                notice.EstimatedDailyRevenue,
                order is null ? (decimal?)null : WrongSaleDetailRules.Money(order.SellAmount)),
            paymentsCount,
            payments.Select(p => new WrongSalePaymentDto(
                p.Id, p.Date, WrongSaleDetailRules.Money(p.Amount), p.Description)).ToList(),
            matches);

        return dto;
    }

    private async Task<IReadOnlyList<WrongSaleMatchDto>> LoadMatchesAsync(
        int branchId, int customerId, string? name, string? phone, CancellationToken ct)
    {
        var normalizedPhone = SalesPhoneNormalizer.ForMatch(phone);
        var phoneSuffix = normalizedPhone.Length >= 7 ? normalizedPhone[^7..] : null;
        var hasTriple = SalesRequestNameSimilarity.HasEnoughTripleName(name);
        var firstToken = hasTriple
            ? (name ?? "").Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
            : null;
        if (phoneSuffix is null && string.IsNullOrWhiteSpace(firstToken))
            return [];

        var candidates = await _db.Customers.AsNoTracking()
            .Where(c => c.BranchId == branchId && c.Id != customerId)
            .Where(c => (phoneSuffix != null && c.PhoneNumber.Contains(phoneSuffix))
                        || (firstToken != null && c.FullName.Contains(firstToken)))
            .OrderBy(c => c.Id)
            .Take(WrongSaleDetailRules.MaxMatchCandidates)
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber })
            .ToListAsync(ct);

        var hits = candidates
            .Select(c => new { c.Id, c.FullName, c.PhoneNumber, Reasons = WrongSaleDetailRules.MatchReasons(name, phone, c.FullName, c.PhoneNumber) })
            .Where(x => x.Reasons.Count > 0)
            .Take(WrongSaleDetailRules.MaxMatches)
            .ToList();
        if (hits.Count == 0)
            return [];

        var ids = hits.Select(h => h.Id).ToList();
        var balances = await CustomerBalanceQuery.ForCustomersAsync(_db, ids, ct);
        var lastSales = await _db.Orders.AsNoTracking()
            .Where(o => ids.Contains(o.CustomerId) && o.ApprovalStatus == OrderApprovalStatus.Approved)
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, Last = g.Max(o => o.SaleDate) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Last, ct);

        return hits.Select(h =>
        {
            balances.TryGetValue(h.Id, out var b);
            return new WrongSaleMatchDto(
                h.Id,
                h.FullName,
                h.PhoneNumber,
                h.Reasons,
                lastSales.GetValueOrDefault(h.Id),
                WrongSaleDetailRules.Money(b?.SellTotal ?? 0d),
                WrongSaleDetailRules.Money(b?.PaidTotal ?? 0d),
                WrongSaleDetailRules.Money(b?.Rounded ?? 0d));
        }).ToList();
    }
}
