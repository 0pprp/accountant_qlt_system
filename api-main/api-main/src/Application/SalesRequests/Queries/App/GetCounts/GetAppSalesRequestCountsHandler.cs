using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Queries.App.GetCounts;

public class GetAppSalesRequestCountsHandler
    : IRequestHandler<GetAppSalesRequestCountsQuery, Result<List<AppSalesRequestBinCount>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAppSalesRequestCountsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<AppSalesRequestBinCount>>> Handle(
        GetAppSalesRequestCountsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null or <= 0)
            return SalesRequestErrors.NotOwner;

        // مفاتيح واجهة الموظف القديمة للتوافق مع GlassyStatusCard
        var uiKeys = new (string UiKey, string ApiTab)[]
        {
            ("pending", SalesRequestTabMap.Incoming),
            ("ready", SalesRequestTabMap.Prepared),
            ("disclosure", SalesRequestTabMap.Inspected),
            ("on_hold", SalesRequestTabMap.Pending),
            ("rejected", SalesRequestTabMap.Rejected),
            ("sold", SalesRequestTabMap.Sold),
        };

        var result = new List<AppSalesRequestBinCount>();
        foreach (var (uiKey, apiTab) in uiKeys)
        {
            var statuses = SalesRequestTabMap.StatusesForTab(apiTab);
            var count = await _db.SalesRequests.AsNoTracking()
                .CountAsync(
                    x => !x.IsDeleted
                         && x.TargetEmployeeId == userId
                         && statuses.Contains(x.Status),
                    cancellationToken);
            result.Add(new AppSalesRequestBinCount
            {
                Key = uiKey,
                Label = SalesRequestTabMap.ArabicLabel(apiTab),
                Count = count
            });
        }

        return result;
    }
}
