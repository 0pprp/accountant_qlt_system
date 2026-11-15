using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;
using Application.SalesRatings.Common;
using Application.SalesRequests.Common;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.ManagementSalesMap.Common;

public class ManagementSalesMapDataService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly DelegateFollowupDataService _followup;

    public ManagementSalesMapDataService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        DelegateFollowupDataService followup)
    {
        _db = db;
        _clock = clock;
        _followup = followup;
    }

    public async Task<IReadOnlyList<int>> AuthorizedBranchIdsAsync(
        ICurrentUserService user,
        CancellationToken ct)
    {
        await Task.CompletedTask;
        return SalesRequestAccess.AuthorizedBranches(user);
    }

    public async Task<(IReadOnlyList<int> BranchIds, string? ProvinceName, Error? Error)> ResolveProvinceBranchesAsync(
        ICurrentUserService user,
        int provinceId,
        CancellationToken ct)
    {
        if (provinceId <= 0)
            return ([], null, ManagementSalesMapErrors.ProvinceNotFound);

        var province = await _db.Provinces.AsNoTracking()
            .Where(p => p.Id == provinceId)
            .Select(p => new { p.Id, p.Name })
            .FirstOrDefaultAsync(ct);
        if (province is null)
            return ([], null, ManagementSalesMapErrors.ProvinceNotFound);

        var authorized = SalesRequestAccess.AuthorizedBranches(user);
        var branchIds = await _db.Branches.AsNoTracking()
            .Where(b => b.ProvinceId == provinceId && authorized.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(ct);

        if (branchIds.Count == 0)
            return ([], province.Name, ManagementSalesMapErrors.ProvinceForbidden);

        return (branchIds, province.Name, null);
    }

    public async Task<SalesMapProvincesResponse> GetProvincesAsync(
        IReadOnlyList<int> authorizedBranches,
        CancellationToken ct)
    {
        if (authorizedBranches.Count == 0)
            return new SalesMapProvincesResponse([]);

        var asOf = _clock.Today;
        var branches = await _db.Branches.AsNoTracking()
            .Where(b => authorizedBranches.Contains(b.Id))
            .Select(b => new { b.Id, b.ProvinceId, ProvinceName = b.Province != null ? b.Province.Name : "" })
            .ToListAsync(ct);

        var byProvince = branches
            .GroupBy(b => b.ProvinceId)
            .Select(g => new
            {
                ProvinceId = g.Key,
                ProvinceName = g.Select(x => x.ProvinceName).FirstOrDefault() ?? $"محافظة {g.Key}",
                BranchIds = g.Select(x => x.Id).ToList()
            })
            .OrderBy(x => x.ProvinceName)
            .ToList();

        var cards = new List<SalesMapProvinceCardDto>(byProvince.Count);
        foreach (var p in byProvince)
        {
            var lists = await _followup.ListActiveListsAsync(p.BranchIds, ct);
            var listIds = lists.Select(l => l.ListId).ToHashSet();
            var (withLoc, withoutLoc) = await CountSalesAsync(p.BranchIds, listIds, ct);
            cards.Add(new SalesMapProvinceCardDto(
                p.ProvinceId,
                string.IsNullOrWhiteSpace(p.ProvinceName) ? $"محافظة {p.ProvinceId}" : p.ProvinceName.Trim(),
                lists.Count,
                withLoc,
                withoutLoc));
        }

        return new SalesMapProvincesResponse(cards);
    }

    public async Task<SalesMapListsResponse> GetListsAsync(
        IReadOnlyList<int> branchIds,
        CancellationToken ct)
    {
        var lists = await _followup.ListActiveListsAsync(branchIds, ct);
        var orderedIds = lists.OrderBy(l => l.ListId).Select(l => l.ListId).ToList();
        var letters = SalesMapListLetters.Assign(orderedIds);

        var result = new List<SalesMapListOptionDto>(lists.Count);
        foreach (var list in lists.OrderBy(l => l.ListId))
        {
            var (withLoc, withoutLoc) = await CountSalesAsync(
                branchIds,
                new HashSet<int> { list.ListId },
                ct);
            result.Add(new SalesMapListOptionDto(
                list.ListId,
                list.ListName,
                list.BranchId,
                letters[list.ListId],
                withLoc,
                withoutLoc));
        }

        return new SalesMapListsResponse(result);
    }

    public async Task<SalesMapPointsResponse> GetPointsAsync(
        IReadOnlyList<int> branchIds,
        IReadOnlyList<int>? listIdsFilter,
        SalesMapStatusFilter statusFilter,
        CancellationToken ct)
    {
        var eligibleLists = await _followup.ListActiveListsAsync(branchIds, ct);
        if (eligibleLists.Count == 0)
            return new SalesMapPointsResponse([]);

        HashSet<int> listIds;
        if (listIdsFilter is null)
        {
            listIds = eligibleLists.Select(l => l.ListId).ToHashSet();
        }
        else if (listIdsFilter.Count == 0)
        {
            return new SalesMapPointsResponse([]);
        }
        else
        {
            var allowed = eligibleLists.Select(l => l.ListId).ToHashSet();
            listIds = listIdsFilter.Where(allowed.Contains).ToHashSet();
            if (listIds.Count == 0)
                return new SalesMapPointsResponse([]);
        }

        var letters = SalesMapListLetters.Assign(
            eligibleLists.OrderBy(l => l.ListId).Select(l => l.ListId));
        var listNameById = eligibleLists.ToDictionary(l => l.ListId, l => l.ListName);
        var asOf = _clock.Today;

        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved
                        && o.OrderListId != null
                        && listIds.Contains(o.OrderListId.Value)
                        && branchIds.Contains(o.BranchId)
                        && o.Customer != null
                        && o.Customer.IsFakeSale == false)
            .Select(o => new
            {
                o.Id,
                o.CustomerId,
                CustomerName = o.Customer!.FullName,
                IsLegal = o.Customer.IsLegal,
                ListId = o.OrderListId!.Value,
                o.SaleDate,
                o.SellAmount,
                o.Location,
                Paid = o.InstallmentPayments.Sum(ip => ip.Amount),
                LastPaymentDate = o.InstallmentPayments
                    .OrderByDescending(ip => ip.Date)
                    .ThenByDescending(ip => ip.Id)
                    .Select(ip => (DateOnly?)ip.Date)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var points = new List<SalesMapPointDto>(rows.Count);
        foreach (var r in rows)
        {
            if (SalesMapGeo.TryGetValid(r.Location, out var lat, out var lng) == false)
                continue;

            var remaining = Math.Round(r.SellAmount - r.Paid, MidpointRounding.AwayFromZero);
            var (code, label) = SalesMapStatusRules.Classify(r.IsLegal, r.LastPaymentDate, r.SaleDate, asOf);
            if (SalesMapStatusRules.MatchesFilter(code, statusFilter) == false)
                continue;

            string? ratingLabel = null;
            string? tier = null;
            if (r.IsLegal == false)
            {
                var score = CustomerRatingCalculator.ScoreFor(
                    r.SellAmount, r.Paid, remaining, r.SaleDate, r.LastPaymentDate, asOf);
                if (score is int s)
                {
                    ratingLabel = CustomerRatingCalculator.LevelName(s);
                    tier = SalesMapStatusRules.RatingTierKey(ratingLabel);
                }
            }

            points.Add(new SalesMapPointDto(
                r.Id,
                r.CustomerId,
                r.CustomerName?.Trim() ?? $"زبون {r.CustomerId}",
                r.ListId,
                listNameById.GetValueOrDefault(r.ListId) ?? $"قائمة {r.ListId}",
                letters.GetValueOrDefault(r.ListId) ?? "؟",
                lat,
                lng,
                code,
                label,
                tier,
                ratingLabel));
        }

        return new SalesMapPointsResponse(points);
    }

    public async Task<Result<SalesMapOrderDetailDto>> GetOrderDetailAsync(
        IReadOnlyList<int> authorizedBranches,
        int orderId,
        CancellationToken ct)
    {
        var asOf = _clock.Today;
        var row = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == orderId
                        && o.ApprovalStatus == OrderApprovalStatus.Approved
                        && authorizedBranches.Contains(o.BranchId)
                        && o.Customer != null)
            .Select(o => new
            {
                o.Id,
                o.CustomerId,
                CustomerName = o.Customer!.FullName,
                IsLegal = o.Customer.IsLegal,
                IsFake = o.Customer.IsFakeSale,
                ListId = o.OrderListId,
                ListName = o.OrderList != null ? o.OrderList.Name : null,
                BranchId = o.BranchId,
                ProvinceId = o.Branch != null ? o.Branch.ProvinceId : 0,
                o.SaleDate,
                o.SellAmount,
                Paid = o.InstallmentPayments.Sum(ip => ip.Amount),
                LastPaymentDate = o.InstallmentPayments
                    .OrderByDescending(ip => ip.Date)
                    .ThenByDescending(ip => ip.Id)
                    .Select(ip => (DateOnly?)ip.Date)
                    .FirstOrDefault(),
                LastPaymentAmount = o.InstallmentPayments
                    .OrderByDescending(ip => ip.Date)
                    .ThenByDescending(ip => ip.Id)
                    .Select(ip => (double?)ip.Amount)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (row is null || row.IsFake)
            return ManagementSalesMapErrors.OrderNotFound;

        if (row.ListId is null)
            return ManagementSalesMapErrors.OrderNotFound;

        // الحرف ضمن كل قوائم المحافظة الأهلية (ثبات الحرف)
        var provinceBranches = await _db.Branches.AsNoTracking()
            .Where(b => b.ProvinceId == row.ProvinceId && authorizedBranches.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(ct);
        var eligible = await _followup.ListActiveListsAsync(provinceBranches, ct);
        var letters = SalesMapListLetters.Assign(eligible.OrderBy(l => l.ListId).Select(l => l.ListId));
        var letter = letters.GetValueOrDefault(row.ListId.Value) ?? "؟";

        var remaining = Math.Round(row.SellAmount - row.Paid, MidpointRounding.AwayFromZero);
        var (_, statusLabel) = SalesMapStatusRules.Classify(row.IsLegal, row.LastPaymentDate, row.SaleDate, asOf);

        string? ratingLabel = null;
        if (row.IsLegal == false)
        {
            var score = CustomerRatingCalculator.ScoreFor(
                row.SellAmount, row.Paid, remaining, row.SaleDate, row.LastPaymentDate, asOf);
            if (score is int s)
                ratingLabel = CustomerRatingCalculator.LevelName(s);
        }

        string? lastDisplay;
        if (row.LastPaymentDate is null || row.LastPaymentAmount is null)
            lastDisplay = "لا يوجد تسديد بعد";
        else
            lastDisplay = null;

        return new SalesMapOrderDetailDto(
            row.Id,
            row.CustomerId,
            row.CustomerName?.Trim() ?? $"زبون {row.CustomerId}",
            row.ListId.Value,
            string.IsNullOrWhiteSpace(row.ListName) ? $"قائمة {row.ListId}" : row.ListName!.Trim(),
            letter,
            statusLabel,
            ratingLabel,
            row.SellAmount,
            row.Paid,
            remaining,
            row.SaleDate,
            row.LastPaymentDate,
            row.LastPaymentAmount,
            lastDisplay);
    }

    private async Task<(int WithLocation, int WithoutLocation)> CountSalesAsync(
        IReadOnlyList<int> branchIds,
        HashSet<int> listIds,
        CancellationToken ct)
    {
        if (branchIds.Count == 0 || listIds.Count == 0)
            return (0, 0);

        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.ApprovalStatus == OrderApprovalStatus.Approved
                        && o.OrderListId != null
                        && listIds.Contains(o.OrderListId.Value)
                        && branchIds.Contains(o.BranchId)
                        && o.Customer != null
                        && o.Customer.IsFakeSale == false)
            .Select(o => o.Location)
            .ToListAsync(ct);

        var with = 0;
        var without = 0;
        foreach (var loc in rows)
        {
            if (SalesMapGeo.TryGetValid(loc, out _, out _))
                with++;
            else
                without++;
        }

        return (with, without);
    }
}
