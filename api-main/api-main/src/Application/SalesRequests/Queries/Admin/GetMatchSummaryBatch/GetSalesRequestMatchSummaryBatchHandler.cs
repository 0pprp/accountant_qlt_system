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
            .Select(x => new { x.Id, x.CustomerName, x.CustomerPhone })
            .ToListAsync(cancellationToken);

        var asOf = _clock.Today;
        var items = new List<SalesMatchSummaryItemDto>(requests.Count);
        foreach (var req in requests)
        {
            var cards = new List<SalesMatchCardDto>(SalesMatchCriteria.All.Length);
            foreach (var key in SalesMatchCriteria.All)
            {
                var customers = await SalesMatchCandidateSelector.LoadCustomerCandidatesAsync(
                    _db, branches, key, req.CustomerName, req.CustomerPhone, cancellationToken);

                var probe = SalesRequestMatchEvaluator.EvaluateCard(
                    key, req.CustomerName, req.CustomerPhone, customers);
                if (probe.State is "insufficientData" or "failed")
                {
                    cards.Add(probe);
                    continue;
                }

                var matchedIds = SalesRequestMatchEvaluator
                    .FilterHits(key, req.CustomerName, req.CustomerPhone, customers)
                    .Select(c => c.Id)
                    .Distinct()
                    .ToList();

                var qualified = await SalesMatchSignificantPayment.LoadQualifiedAsync(
                    _db, matchedIds, asOf, cancellationToken);
                var count = matchedIds.Count(id => qualified.ContainsKey(id));
                var label = SalesMatchCardLabels.For(key);
                cards.Add(new SalesMatchCardDto
                {
                    Key = key,
                    Label = label,
                    State = SalesMatchCardLabels.StateName(
                        count > 0 ? SalesMatchCardState.Results : SalesMatchCardState.NoResults),
                    Count = count
                });
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
