using Application.Common.Interfaces;
using Domain.Entities.AttachmentAggregate.Enums;
using Domain.Entities.CustomerAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.FollowerWork.Common;

public static class FollowerShopPhotoSources
{
    public const string Imported = "imported";
    public const string App = "app";

    public static bool IsKnown(string? source) => source is Imported or App;

    public static Dictionary<int, FollowerShopPhotoRef> LatestPerCustomer(IEnumerable<FollowerShopPhotoRef> photos)
        => photos
            .GroupBy(p => p.CustomerId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(p => p.CapturedAtUtc ?? DateTimeOffset.MinValue)
                    .ThenBy(p => p.Source == App ? 0 : 1)
                    .ThenByDescending(p => p.PhotoId)
                    .First());
}

public sealed record FollowerShopPhotoRef(int CustomerId, int PhotoId, DateTimeOffset? CapturedAtUtc, string Source);

public sealed record ShopCaptureCustomerRow(
    int CustomerId,
    string CustomerName,
    string? ShopName,
    string? PhoneNumber,
    int ListId,
    double AmountRemaining,
    bool HasLocation,
    double? Latitude,
    double? Longitude,
    FollowerShopPhotoRef? Photo)
{
    public bool HasPhoto => Photo is not null;
    public bool IsComplete => HasLocation && HasPhoto;
}

public static class ShopCaptureProgressQuery
{
    public const double RemainingRoundingUnit = 1000;
    public const string ArchiveListName = "قائمة أرشيف";

    public static bool IsArchiveList(string? listName)
        => string.Equals(listName?.Trim(), ArchiveListName, StringComparison.Ordinal);

    public static bool HasRoundedOpenBalance(double amountRemaining)
        => Math.Round(amountRemaining / RemainingRoundingUnit, MidpointRounding.AwayFromZero) * RemainingRoundingUnit > 0;

    public static bool IsUsableCoordinate(double? lat, double? lng)
        => lat is double a
           && lng is double b
           && FollowerLocationRules.IsValidCoordinate(a, b)
           && (Math.Abs(a) < 1e-9 && Math.Abs(b) < 1e-9) == false;

    public static async Task<List<ShopCaptureCustomerRow>> LoadAsync(
        IApplicationDbContext db,
        IReadOnlyList<int> listIds,
        CancellationToken ct)
    {
        if (listIds.Count == 0)
            return [];

        var listNames = await db.OrderLists.AsNoTracking()
            .Where(l => listIds.Contains(l.Id))
            .Select(l => new { l.Id, l.Name })
            .ToListAsync(ct);
        var captureListIds = listNames.Where(l => IsArchiveList(l.Name) == false).Select(l => l.Id).ToList();
        if (captureListIds.Count == 0)
            return [];

        var facts = await FollowerFactsQuery.LoadAsync(db, captureListIds, null, null, ct);
        var open = facts
            .Where(f => HasRoundedOpenBalance(f.AmountRemaining) && f.IsLegal == false && f.OrderListId is > 0)
            .ToList();
        if (open.Count == 0)
            return [];

        var customerIds = open.Select(f => f.CustomerId).Distinct().ToList();

        var customers = await db.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                c.FullName,
                ShopName = c.Business.Name,
                c.PhoneNumber
            })
            .ToDictionaryAsync(c => c.Id, ct);

        var captured = await db.CustomerLocations.AsNoTracking()
            .Where(x => customerIds.Contains(x.CustomerId) && x.Source == CustomerLocationSource.Captured)
            .GroupBy(x => x.CustomerId)
            .Select(g => g.OrderByDescending(x => x.CapturedAtUtc).ThenByDescending(x => x.Id)
                .Select(x => new { x.CustomerId, x.Latitude, x.Longitude })
                .First())
            .ToDictionaryAsync(x => x.CustomerId, ct);

        var importedPhotos = await db.CustomerShopPhotos.AsNoTracking()
            .Where(x => customerIds.Contains(x.CustomerId))
            .GroupBy(x => x.CustomerId)
            .Select(g => g.OrderByDescending(x => x.CapturedAtUtc).ThenByDescending(x => x.Id)
                .Select(x => new { x.CustomerId, x.Id, CapturedAtUtc = (DateTimeOffset?)x.CapturedAtUtc })
                .First())
            .ToListAsync(ct);

        var appPhotos = await db.Attachments.AsNoTracking()
            .Where(x => x.CustomerId != null
                        && customerIds.Contains(x.CustomerId.Value)
                        && x.Type == AttachmentType.CustomerShopPhoto)
            .GroupBy(x => x.CustomerId!.Value)
            .Select(g => g.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Select(x => new { CustomerId = x.CustomerId!.Value, x.Id, CapturedAtUtc = x.CreatedAt })
                .First())
            .ToListAsync(ct);

        var photos = FollowerShopPhotoSources.LatestPerCustomer(
            importedPhotos
                .Select(p => new FollowerShopPhotoRef(p.CustomerId, p.Id, p.CapturedAtUtc, FollowerShopPhotoSources.Imported))
                .Concat(appPhotos.Select(p => new FollowerShopPhotoRef(p.CustomerId, p.Id, p.CapturedAtUtc, FollowerShopPhotoSources.App))));

        var rows = new List<ShopCaptureCustomerRow>(open.Count);
        foreach (var fact in open)
        {
            if (customers.TryGetValue(fact.CustomerId, out var c) == false)
                continue;

            double? lat = null;
            double? lng = null;
            if (captured.TryGetValue(c.Id, out var cap) && IsUsableCoordinate(cap.Latitude, cap.Longitude))
            {
                lat = cap.Latitude;
                lng = cap.Longitude;
            }

            photos.TryGetValue(c.Id, out var photo);
            rows.Add(new ShopCaptureCustomerRow(
                c.Id,
                c.FullName,
                c.ShopName,
                c.PhoneNumber,
                fact.OrderListId!.Value,
                fact.AmountRemaining,
                lat is not null && lng is not null,
                lat,
                lng,
                photo));
        }

        return rows;
    }
}
