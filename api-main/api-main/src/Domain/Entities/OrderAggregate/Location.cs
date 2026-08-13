using Domain.Common;

namespace Domain.Entities.OrderAggregate;

public class Location : ValueObject
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public Location(double latitude, double longitude)
    {
        ValidateCoordinates(latitude, longitude);
        
        Latitude = latitude;
        Longitude = longitude;
    }

    private static void ValidateCoordinates(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString()
    {
        return $"({Latitude}, {Longitude})";
    }
}