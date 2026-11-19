using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.DelegateFollowup.Queries.Admin.GetLocations;

public record GetDelegateFollowupLocationsQuery(int ListId, int BranchId) : IRequest<Result<DelegateFollowupLocationsDto>>;

public class DelegateFollowupLocationsDto
{
    public int ListId { get; set; }
    public string ListName { get; set; } = "";
    public double Collected30d { get; set; }
    public double AvgDailyCollected30d => Math.Round(Collected30d / 30, 0, MidpointRounding.AwayFromZero);
    public List<DelegateFollowupLocationDayDto> Daily30d { get; set; } = [];
    public List<DelegateFollowupLocationDto> Customers { get; set; } = [];
}

public class DelegateFollowupLocationDayDto
{
    public DateOnly Date { get; set; }
    public double Amount { get; set; }
    public int Count { get; set; }
}

public class DelegateFollowupLocationDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string? ShopName { get; set; }
    public string? PhoneNumber { get; set; }
    public double AmountRemaining { get; set; }
    public DateOnly? LastPaymentDate { get; set; }
    public bool IsActive { get; set; }
    public string RatingLabel { get; set; } = "";
    public string? Address { get; set; }
    public double? FollowerLatitude { get; set; }
    public double? FollowerLongitude { get; set; }
    public double? SalesLatitude { get; set; }
    public double? SalesLongitude { get; set; }
}

public class GetDelegateFollowupLocationsHandler(IApplicationDbContext db, ICurrentUserService actor, IDateTimeProvider clock, DelegateFollowupDataService data)
    : IRequestHandler<GetDelegateFollowupLocationsQuery, Result<DelegateFollowupLocationsDto>>
{
    public async Task<Result<DelegateFollowupLocationsDto>> Handle(GetDelegateFollowupLocationsQuery request, CancellationToken ct)
    {
        var error = DelegateFollowupAccess.ValidateBranch(actor, request.BranchId);
        if (error is not null) return error.Value;
        var list = await db.OrderLists.AsNoTracking().Where(l => l.Id == request.ListId && l.BranchId == request.BranchId).Select(l => new { l.Id, l.Name }).FirstOrDefaultAsync(ct);
        if (list is null) return DelegateFollowupErrors.ListNotFound;
        var result = await data.GetCustomersAsync(request.ListId, request.BranchId, ct);
        if (result is null) return DelegateFollowupErrors.ListNotFound;
        var ids = result.Customers.Where(c => c.IsLegal == false).Select(c => c.CustomerId).ToList();
        var locations = await db.Customers.AsNoTracking().Where(c => ids.Contains(c.Id) && c.BranchId == request.BranchId && c.IsFakeSale == false)
            .Select(c => new { c.Id, c.Business.GpsLatitude, c.Business.GpsLongitude }).ToListAsync(ct);
        var sales = await db.Orders.AsNoTracking().Where(o => o.OrderListId == request.ListId && o.BranchId == request.BranchId && o.Location != null && ids.Contains(o.CustomerId))
            .Select(o => new { o.Id, o.CustomerId, o.Location!.Latitude, o.Location.Longitude }).ToListAsync(ct);
        var salesByCustomer = sales.GroupBy(o => o.CustomerId).ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.Id).First());
        var facts = result.Customers.ToDictionary(c => c.CustomerId);
        var today = clock.Today;
        var from = today.AddDays(-29);
        var daily = await db.InstallmentPayments.AsNoTracking().Where(p => p.Order != null && p.Order.OrderListId == request.ListId && p.Order.BranchId == request.BranchId && p.Date >= from && p.Date <= today)
            .GroupBy(p => p.Date).Select(g => new { Date = g.Key, Amount = g.Sum(p => p.Amount), Count = g.Count() }).ToDictionaryAsync(g => g.Date, ct);
        return new DelegateFollowupLocationsDto
        {
            ListId = list.Id, ListName = list.Name, Collected30d = daily.Values.Sum(d => d.Amount),
            Daily30d = Enumerable.Range(0, 30).Select(i => from.AddDays(i)).Select(d => new DelegateFollowupLocationDayDto { Date = d, Amount = daily.TryGetValue(d, out var v) ? v.Amount : 0, Count = daily.TryGetValue(d, out var c) ? c.Count : 0 }).ToList(),
            Customers = locations.Select(c => {
                var f = facts[c.Id]; salesByCustomer.TryGetValue(c.Id, out var sale);
                return new DelegateFollowupLocationDto { CustomerId = c.Id, CustomerName = f.CustomerName, ShopName = f.ShopName, PhoneNumber = f.PhoneNumber, AmountRemaining = f.AmountRemaining, LastPaymentDate = f.LastPaymentDate, IsActive = f.IsActive, Address = f.Address, RatingLabel = f.RealRatingLabel, FollowerLatitude = c.GpsLatitude, FollowerLongitude = c.GpsLongitude, SalesLatitude = sale?.Latitude, SalesLongitude = sale?.Longitude };
            }).ToList()
        };
    }
}
