using Application.Common.Interfaces;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRecords.Common;

public class SalesRecordsDataService
{
    private static readonly HashSet<string> AllowedSorts =
        new(StringComparer.OrdinalIgnoreCase) { "saleDate", "sellAmount", "customer", "seller", "id" };

    private readonly IApplicationDbContext _db;

    public SalesRecordsDataService(IApplicationDbContext db)
    {
        _db = db;
    }

    public static Error? ValidateSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return null;
        return AllowedSorts.Contains(sort.Trim()) ? null : SalesRecordsErrors.InvalidSort;
    }

    public async Task<Result<ListSalesRecordsResponse>> ListAsync(
        IReadOnlyList<int> branchScope,
        int? sellerId,
        string? search,
        DateOnly? saleDateFrom,
        DateOnly? saleDateTo,
        int page,
        int pageSize,
        string sort,
        CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // بيع منجز = Order معتمد — ليس SalesRequest.Completed
        var query = _db.Orders.AsNoTracking()
            .Where(o => branchScope.Contains(o.BranchId)
                        && o.ApprovalStatus == OrderApprovalStatus.Approved);

        if (sellerId is > 0)
            query = query.Where(o => o.SellerId == sellerId.Value);

        if (saleDateFrom is not null)
            query = query.Where(o => o.SaleDate != null && o.SaleDate >= saleDateFrom);
        if (saleDateTo is not null)
            query = query.Where(o => o.SaleDate != null && o.SaleDate <= saleDateTo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(o =>
                (o.Customer != null && o.Customer.FullName.Contains(term))
                || (o.Customer != null && o.Customer.PhoneNumber != null && o.Customer.PhoneNumber.Contains(term))
                || (o.Seller != null && o.Seller.FullName != null && o.Seller.FullName.Contains(term))
                || o.Id.ToString() == term);
        }

        var totalCount = await query.CountAsync(ct);

        var totalsRaw = await query
            .Select(o => new
            {
                o.SellAmount,
                o.PrepaymentAmount,
                Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
            })
            .ToListAsync(ct);

        var totalSell = totalsRaw.Sum(x => x.SellAmount);
        var totalPrepay = totalsRaw.Sum(x => x.PrepaymentAmount);
        var totalReceipts = totalsRaw.Sum(x => x.Receipts);
        var totalRemaining = Math.Round(totalSell - totalReceipts, MidpointRounding.AwayFromZero);

        IOrderedQueryable<Domain.Entities.OrderAggregate.Order> ordered = sort.ToLowerInvariant() switch
        {
            "sellamount" => query.OrderByDescending(o => o.SellAmount).ThenByDescending(o => o.Id),
            "customer" => query.OrderBy(o => o.Customer!.FullName).ThenByDescending(o => o.Id),
            "seller" => query.OrderBy(o => o.Seller!.FullName).ThenByDescending(o => o.Id),
            "id" => query.OrderByDescending(o => o.Id),
            _ => query.OrderByDescending(o => o.SaleDate).ThenByDescending(o => o.Id),
        };

        var pageRows = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.SaleDate,
                o.ApprovalStatus,
                o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.FullName : null,
                CustomerPhone = o.Customer != null ? o.Customer.PhoneNumber : null,
                o.SellerId,
                SellerName = o.Seller != null ? o.Seller.FullName : null,
                o.BranchId,
                BranchName = o.Branch != null ? o.Branch.Name : null,
                o.SellAmount,
                o.PrepaymentAmount,
                o.DailyInstallmentAmount,
                Receipts = o.InstallmentPayments.Sum(ip => ip.Amount),
            })
            .ToListAsync(ct);

        var orderIds = pageRows.Select(r => r.Id).ToList();
        var linked = orderIds.Count == 0
            ? new Dictionary<int, int>()
            : await _db.SalesRequests.AsNoTracking()
                .Where(r => r.LinkedOrderId != null && orderIds.Contains(r.LinkedOrderId.Value) && !r.IsDeleted)
                .GroupBy(r => r.LinkedOrderId!.Value)
                .Select(g => new { OrderId = g.Key, RequestId = g.Min(x => x.Id) })
                .ToDictionaryAsync(x => x.OrderId, x => x.RequestId, ct);

        var items = pageRows.Select(r =>
        {
            var remaining = Math.Round(r.SellAmount - r.Receipts, MidpointRounding.AwayFromZero);
            linked.TryGetValue(r.Id, out var reqId);
            return new SalesRecordRowDto
            {
                OrderId = r.Id,
                SaleDate = r.SaleDate,
                ApprovalStatus = r.ApprovalStatus.ToString(),
                CustomerId = r.CustomerId,
                CustomerName = r.CustomerName ?? $"#{r.CustomerId}",
                CustomerPhone = r.CustomerPhone,
                SellerId = r.SellerId,
                SellerName = r.SellerName,
                BranchId = r.BranchId,
                BranchName = r.BranchName ?? $"#{r.BranchId}",
                SellAmount = r.SellAmount,
                PrepaymentAmount = r.PrepaymentAmount,
                DailyInstallmentAmount = r.DailyInstallmentAmount,
                ReceiptsTotal = r.Receipts,
                RemainingAmount = remaining,
                LinkedSalesRequestId = reqId > 0 ? reqId : null,
            };
        }).ToList();

        return new ListSalesRecordsResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Totals = new SalesRecordsTotalsDto
            {
                TotalSellAmount = totalSell,
                TotalPrepaymentAmount = totalPrepay,
                TotalReceipts = totalReceipts,
                TotalRemaining = totalRemaining,
                ScopeNote = "مجاميع كل النتائج المطابقة للفلاتر — ليست مجموع الصفحة فقط",
            },
            BranchScope = branchScope,
            CurrencyNote = "د.ع",
        };
    }
}
