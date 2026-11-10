using Application.BranchDeclining.Common;
using Application.BranchVisits.Common;
using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Application.SalesRequests.Common;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.BranchDeclining.Queries;

public sealed class ListBranchDecliningQuery : IRequest<Result<BranchDecliningResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public sealed class ListBranchDecliningHandler
    : IRequestHandler<ListBranchDecliningQuery, Result<BranchDecliningResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public ListBranchDecliningHandler(
        IApplicationDbContext db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _clock = clock;
    }

    public async Task<Result<BranchDecliningResponse>> Handle(
        ListBranchDecliningQuery request, CancellationToken ct)
    {
        if (_current.HasPermission(SeedPermissions.BranchDeclining.Read) == false)
            return BranchDecliningErrors.Forbidden;

        var visitResult = await BranchVisitRequirement.RequireOpenAsync(
            _db, _current, requiredBranchId: null, ct);
        if (visitResult.IsFailed)
            return visitResult.Error!.Value;

        var visitBranchId = visitResult.Data!.BranchId;
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var asOf = DateOnly.FromDateTime(_clock.UtcNowOffset.ToOffset(TimeSpan.FromHours(3)).DateTime);
        var fromDate = asOf.AddDays(-BranchDeclineRules.LookbackDays);

        var listIds = await _db.OrderLists.AsNoTracking()
            .Where(l => l.BranchId == visitBranchId)
            .Select(l => new { l.Id, l.BranchId })
            .ToListAsync(ct);
        if (listIds.Count == 0)
            return new BranchDecliningResponse([], 0, false, page, pageSize);

        var orderListIds = listIds.Select(x => x.Id).ToList();
        var branchByList = listIds.ToDictionary(x => x.Id, x => x.BranchId);

        var snaps = await _db.FollowerCustomerRatingSnapshots.AsNoTracking()
            .Where(s => orderListIds.Contains(s.OrderListId)
                        && s.CaptureDate >= fromDate
                        && s.CaptureDate <= asOf)
            .Select(s => new
            {
                s.CustomerId,
                s.CustomerName,
                s.PhoneNumber,
                s.OrderListId,
                s.RatingLabel,
                s.Score,
                s.CaptureDate
            })
            .ToListAsync(ct);

        var historyInsufficient = snaps.Count == 0;
        var rows = new List<BranchDecliningRowDto>();
        foreach (var g in snaps.GroupBy(s => s.CustomerId))
        {
            var ordered = g.OrderByDescending(x => x.CaptureDate).ToList();
            var cur = ordered[0];
            var priorExcellent = ordered
                .Skip(1)
                .FirstOrDefault(h =>
                    FollowerDeclineRules.NormalizeBand(h.RatingLabel) == FollowerDeclineRules.Excellent
                    && h.CaptureDate < cur.CaptureDate);
            if (priorExcellent is null)
                continue;

            var days = cur.CaptureDate.DayNumber - priorExcellent.CaptureDate.DayNumber;
            if (BranchDeclineRules.IsDeclining(priorExcellent.RatingLabel, cur.RatingLabel, days) == false)
                continue;

            var branchId = branchByList.GetValueOrDefault(cur.OrderListId);
            rows.Add(new BranchDecliningRowDto(
                cur.CustomerId,
                cur.CustomerName ?? "",
                cur.PhoneNumber,
                branchId,
                FollowerDeclineRules.NormalizeBand(priorExcellent.RatingLabel),
                FollowerDeclineRules.NormalizeBand(cur.RatingLabel),
                cur.Score,
                priorExcellent.CaptureDate,
                cur.CaptureDate,
                days));
        }

        rows = rows.OrderByDescending(r => r.CurrentCaptureDate).ThenBy(r => r.CustomerName).ToList();
        var total = rows.Count;
        var pageItems = rows.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new BranchDecliningResponse(pageItems, total, historyInsufficient && total == 0, page, pageSize);
    }
}
