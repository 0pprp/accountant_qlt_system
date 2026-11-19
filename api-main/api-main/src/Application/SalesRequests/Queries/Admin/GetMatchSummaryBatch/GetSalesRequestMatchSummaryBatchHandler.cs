using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Services.SalesMatching;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.Admin.GetMatchSummaryBatch;

public class GetSalesRequestMatchSummaryBatchHandler
    : IRequestHandler<GetSalesRequestMatchSummaryBatchQuery, Result<GetSalesRequestMatchSummaryBatchResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetSalesRequestMatchSummaryBatchHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<GetSalesRequestMatchSummaryBatchResponse>> Handle(
        GetSalesRequestMatchSummaryBatchQuery request,
        CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var ids = (request.RequestIds ?? []).Distinct().ToList();

        if (ids.Count > SalesRequestMatchBatchLimits.MaxRequestIds)
            return SalesRequestErrors.BatchLimitExceeded;

        if (ids.Count == 0 || branches.Count == 0)
            return new GetSalesRequestMatchSummaryBatchResponse { Items = [] };

        var requests = await _db.SalesRequests.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted && branches.Contains(x.BranchId))
            .Select(x => new { x.Id, x.CustomerName, x.CustomerPhone, x.BranchId, x.ProvinceId })
            .ToListAsync(cancellationToken);

        var candidatesByScope = new Dictionary<string, List<SalesMatchCandidate>>();
        var items = new List<SalesMatchSummaryItemDto>(requests.Count);
        foreach (var req in requests)
        {
            var matchBranches = await SalesMatchCandidateSelector.ResolveProvinceBranchIdsAsync(
                _db, req.BranchId, req.ProvinceId, cancellationToken);
            var scopeKey = string.Join(",", matchBranches.OrderBy(id => id));
            if (!candidatesByScope.TryGetValue(scopeKey, out var customers))
            {
                customers = await SalesMatchCandidateSelector.LoadCustomerScopeAsync(
                    _db, matchBranches, cancellationToken);
                candidatesByScope[scopeKey] = customers;
            }
            var cards = new List<SalesMatchCardDto>(SalesMatchCriteria.All.Length);
            foreach (var key in SalesMatchCriteria.All)
            {

                cards.Add(SalesRequestMatchEvaluator.EvaluateCard(
                    key, req.CustomerName, req.CustomerPhone, customers));
            }

            items.Add(new SalesMatchSummaryItemDto
            {
                RequestId = req.Id,
                Cards = cards
            });
        }

        return new GetSalesRequestMatchSummaryBatchResponse { Items = items };
    }
}
