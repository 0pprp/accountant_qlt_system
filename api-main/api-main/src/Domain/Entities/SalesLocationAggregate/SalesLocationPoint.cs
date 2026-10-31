using Domain.Common;
using Domain.Entities.BranchAggregate;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.SalesLocationAggregate;

public class SalesLocationPoint : Entity
{
    public SalesLocationPoint(
        int userId,
        int? branchId,
        double latitude,
        double longitude,
        DateTimeOffset recordedAtUtc,
        bool isLive)
    {
        UserId = userId;
        BranchId = branchId;
        Latitude = latitude;
        Longitude = longitude;
        RecordedAtUtc = recordedAtUtc;
        IsLive = isLive;
    }

    public int UserId { get; set; }
    public int? BranchId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTimeOffset RecordedAtUtc { get; set; }
    public bool IsLive { get; set; }

    public User? User { get; set; }
    public Branch? Branch { get; set; }
}
