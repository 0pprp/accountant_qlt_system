using Application.Common.Interfaces;
using Domain.Services.SalesMatching;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Common;

/// <summary>
/// اختيار مرشحي المطابقة بدل تحميل كل الزبائن/الطلبات إلى الذاكرة.
/// نطاق البحث = كل فروع محافظة الطلب (مثل مدينة SalesHaider).
/// الفهارس: Customers.PhoneNumber / Customers.FullName؛ SalesRequests.CustomerPhone / CustomerName.
/// </summary>
public static class SalesMatchCandidateSelector
{
    public const int MaxCandidateFetch = 2000;

    /// <summary>
    /// فروع محافظة الطلب بالكامل — بعد التأكد أن للمستخدم صلاحية على فرع الطلب نفسه.
    /// </summary>
    public static async Task<List<int>> ResolveProvinceBranchIdsAsync(
        IApplicationDbContext db,
        int requestBranchId,
        int? requestProvinceId,
        CancellationToken cancellationToken)
    {
        var provinceId = requestProvinceId;
        if (provinceId is null or <= 0)
        {
            provinceId = await db.Branches.AsNoTracking()
                .Where(b => b.Id == requestBranchId)
                .Select(b => (int?)b.ProvinceId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (provinceId is null or <= 0)
            return [requestBranchId];

        var ids = await db.Branches.AsNoTracking()
            .Where(b => b.ProvinceId == provinceId.Value)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        return ids.Count > 0 ? ids : [requestBranchId];
    }

    public static async Task<List<SalesMatchCandidate>> LoadCustomerCandidatesAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branchIds,
        string criterion,
        string? queryName,
        string? queryPhone,
        CancellationToken cancellationToken)
    {
        if (branchIds.Count == 0)
            return [];

        var candidates = await LoadCustomerScopeAsync(db, branchIds, cancellationToken);
        return SalesRequestMatchEvaluator.FilterHits(criterion, queryName, queryPhone, candidates).ToList();
    }

    public static Task<List<SalesMatchCandidate>> LoadCustomerScopeAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branchIds,
        CancellationToken cancellationToken)
        => db.Customers.AsNoTracking()
            .Where(x => branchIds.Contains(x.BranchId))
            .OrderBy(x => x.Id)
            .Select(x => new SalesMatchCandidate
            {
                Id = x.Id,
                Name = x.FullName,
                Phone = x.PhoneNumber,
                BranchId = x.BranchId
            })
            .ToListAsync(cancellationToken);

    public static async Task<List<SalesMatchCandidate>> LoadPeerRequestCandidatesAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<int> branchIds,
        int excludeRequestId,
        string? queryName,
        string? queryPhone,
        CancellationToken cancellationToken)
    {
        if (branchIds.Count == 0)
            return [];

        var query = db.SalesRequests.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Id != excludeRequestId && branchIds.Contains(x.BranchId));
        // تضييق أولي بالهاتف أو جزء من الاسم عند توفرهما
        var phoneNeedle = SalesPhoneNormalizer.ForMatch(queryPhone);
        if (!string.IsNullOrEmpty(phoneNeedle) && phoneNeedle.Length >= 7)
        {
            var suffix = phoneNeedle[^7..];
            query = query.Where(x => x.CustomerPhone != null && x.CustomerPhone.Contains(suffix));
        }
        else
        {
            var token = FirstUsefulToken(queryName);
            if (!string.IsNullOrEmpty(token))
                query = query.Where(x => x.CustomerName.Contains(token));
        }

        return await query
            .OrderBy(x => x.Id)
            .Take(MaxCandidateFetch)
            .Select(x => new SalesMatchCandidate
            {
                Id = x.Id,
                Name = x.CustomerName,
                Phone = x.CustomerPhone,
                BranchId = x.BranchId
            })
            .ToListAsync(cancellationToken);
    }

    private static string? FirstUsefulToken(string? queryName)
    {
        var tokens = SalesRequestNameSimilarity.MatchingTokens(queryName);
        return tokens.FirstOrDefault(t => t.Length >= 2);
    }
}
