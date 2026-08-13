namespace Domain.Entities.CustomerAggregate;

public record Business
{
    public Business(string name, string address, string nearestKnownLocation)
    {
        Name = name;
        Address = address;
        NearestKnownLocation = nearestKnownLocation;
    }

    public string Name { get; set; }
    public string Address { get; set; }
    public string NearestKnownLocation { get; set; }
}