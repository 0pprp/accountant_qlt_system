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

    public GetSalesRequestMatchSummaryBatchHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
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

        var items = new List<SalesMatchSummaryItemDto>(requests.Count);
        foreach (var req in requests)
        {
            var cards = new List<SalesMatchCardDto>(SalesMatchCriteria.All.Length);
            foreach (var key in SalesMatchCriteria.All)
            {
                // مرشحون مضيّقون لكل معيار — لا تحميل قاعدة الزبائن كاملة لكل دفعة
                var customers = await SalesMatchCandidateSelector.LoadCustomerCandidatesAsync(
                    _db, branches, key, req.CustomerName, req.CustomerPhone, cancellationToken);
                cards.Add(SalesRequestMatchEvaluator.EvaluateCard(
                    key, req.CustomerName, req.CustomerPhone, customers));
            }

            items.Add(new SalesMatchSummaryItemDto
            {
                RequestId = req.Id,
                Cards = cards
            });
        }

        // طلبات غير موجودة/محجوبة لا تُعاد — لا تكشف الوجود عبر العدد
        return new GetSalesRequestMatchSummaryBatchResponse { Items = items };
    }
}
