namespace Domain.Entities.CustomerAggregate;

public record Business
{
    public Business(
        string name,
        string address,
        string nearestKnownLocation,
        double? gpsLatitude = null,
        double? gpsLongitude = null)
    {
        Name = name;
        Address = address;
        NearestKnownLocation = nearestKnownLocation;
        GpsLatitude = gpsLatitude;
        GpsLongitude = gpsLongitude;
    }

    public string Name { get; set; }
    public string Address { get; set; }
    public string NearestKnownLocation { get; set; }

    /// <summary>GPS محفوظ على بروفايل الزبون (من مسؤول اللجنة) — منفصل عن Order.Location.</summary>
    public double? GpsLatitude { get; set; }

    public double? GpsLongitude { get; set; }

    public bool HasGps =>
        GpsLatitude is double lat
        && GpsLongitude is double lng
        && !double.IsNaN(lat)
        && !double.IsNaN(lng)
        && !(Math.Abs(lat) < 1e-9 && Math.Abs(lng) < 1e-9);

    public void SetGps(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));
        GpsLatitude = latitude;
        GpsLongitude = longitude;
    }
}