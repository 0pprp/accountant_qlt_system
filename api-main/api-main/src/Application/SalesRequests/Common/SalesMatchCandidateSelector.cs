using Application.Common.Interfaces;
using Domain.Services.SalesMatching;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Common;

/// <summary>
/// اختيار مرشحي المطابقة بدل تحميل كل الزبائن/الطلبات إلى الذاكرة.
/// الفهارس: Customers.PhoneNumber / Customers.FullName؛ SalesRequests.CustomerPhone / CustomerName.
/// </summary>
public static class SalesMatchCandidateSelector
{
    public const int MaxCandidateFetch = 2000;

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

        var query = db.Customers.AsNoTracking()
            .Where(x => branchIds.Contains(x.BranchId));

        query = ApplyCustomerNarrowing(query, criterion, queryName, queryPhone);

        return await query
            .OrderBy(x => x.Id)
            .Take(MaxCandidateFetch)
            .Select(x => new SalesMatchCandidate
            {
                Id = x.Id,
                Name = x.FullName,
                Phone = x.PhoneNumber,
                BranchId = x.BranchId
            })
            .ToListAsync(cancellationToken);
    }

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

    private static IQueryable<Domain.Entities.CustomerAggregate.Customer> ApplyCustomerNarrowing(
        IQueryable<Domain.Entities.CustomerAggregate.Customer> query,
        string criterion,
        string? queryName,
        string? queryPhone)
    {
        if (criterion == SalesMatchCriteria.Phone)
        {
            var phoneNeedle = SalesPhoneNormalizer.ForMatch(queryPhone);
            if (string.IsNullOrEmpty(phoneNeedle) || phoneNeedle.Length < 7)
                return query.Where(_ => false);

            var suffix = phoneNeedle[^7..];
            return query.Where(x => x.PhoneNumber != null && x.PhoneNumber.Contains(suffix));
        }

        var token = criterion switch
        {
            SalesMatchCriteria.TripleName => SalesRequestNameSimilarity.MatchingTokens(queryName).ElementAtOrDefault(0),
            SalesMatchCriteria.FatherGrandfather => SalesRequestNameSimilarity.MatchingTokens(queryName).ElementAtOrDefault(1),
            SalesMatchCriteria.SonFather => SalesRequestNameSimilarity.MatchingTokens(queryName).ElementAtOrDefault(0),
            _ => FirstUsefulToken(queryName)
        };

        if (string.IsNullOrEmpty(token) || token.Length < 2)
            return query.Where(_ => false);

        return query.Where(x => x.FullName.Contains(token));
    }

    private static string? FirstUsefulToken(string? queryName)
    {
        var tokens = SalesRequestNameSimilarity.MatchingTokens(queryName);
        return tokens.FirstOrDefault(t => t.Length >= 2);
    }
}
