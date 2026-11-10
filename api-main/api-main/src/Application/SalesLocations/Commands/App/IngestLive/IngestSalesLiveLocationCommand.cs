using Domain.Entities.SalesLocationAggregate.Enums;

namespace Application.SalesLocations.Commands.App.IngestLive;

public record IngestSalesLiveLocationCommand : IRequest<Result<IngestSalesLiveLocationResponse>>
{
    public int UserId { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double? AccuracyMeters { get; set; }
    public DateTimeOffset? CapturedAtUtc { get; set; }
    public string? ClientEventId { get; set; }
    /// <summary>يُضبط من الـController فقط — لا من جسم الطلب.</summary>
    public SalesLocationActorKind ActorKind { get; set; } = SalesLocationActorKind.Unknown;
}

public record IngestSalesLiveLocationDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double? AccuracyMeters { get; set; }
    public double? Accuracy { get; set; }
    public DateTimeOffset? CapturedAtUtc { get; set; }
    public string? ClientEventId { get; set; }
}

public record IngestSalesLiveLocationResponse
{
    public int Id { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double? AccuracyMeters { get; set; }
    public DateTimeOffset CapturedAtUtc { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; }
    public bool Duplicate { get; set; }
}
