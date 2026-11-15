using Domain.Entities.OrderAggregate;

namespace Application.ManagementSalesMap.Common;

public static class SalesMapGeo
{
    public static bool TryGetValid(Location? location, out double lat, out double lng)
    {
        lat = 0;
        lng = 0;
        if (location is null)
            return false;

        lat = location.Latitude;
        lng = location.Longitude;
        if (double.IsNaN(lat) || double.IsNaN(lng) || double.IsInfinity(lat) || double.IsInfinity(lng))
            return false;
        if (lat is < -90 or > 90 || lng is < -180 or > 180)
            return false;
        // قيمة وهمية شائعة في المشروع
        if (Math.Abs(lat) < 1e-9 && Math.Abs(lng) < 1e-9)
            return false;
        return true;
    }
}
