using Domain.Entities.SalesLocationAggregate.Enums;

namespace Application.SalesLocations.Commands.App.IngestBatch;

public record IngestSalesLocationBatchCommand : IRequest<Result<IngestSalesLocationBatchResponse>>
{
    public int UserId { get; set; }
    public required List<SalesLocationPointDto> Points { get; set; }
    /// <summary>يُضبط من الـController فقط — لا من جسم الطلب.</summary>
    public SalesLocationActorKind ActorKind { get; set; } = SalesLocationActorKind.Unknown;
}

public record SalesLocationPointDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public DateTimeOffset RecordedAtUtc { get; set; }
    public double? AccuracyMeters { get; set; }
    public string? ClientEventId { get; set; }
}

public record IngestSalesLocationBatchResponse
{
    public int AcceptedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int RejectedCount { get; set; }
}
