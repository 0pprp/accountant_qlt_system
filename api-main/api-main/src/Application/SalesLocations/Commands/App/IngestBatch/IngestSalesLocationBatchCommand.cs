namespace Application.SalesLocations.Commands.App.IngestBatch;

public record IngestSalesLocationBatchCommand : IRequest<Result<IngestSalesLocationBatchResponse>>
{
    public int UserId { get; set; }
    public required List<SalesLocationPointDto> Points { get; set; }
}

public record SalesLocationPointDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public DateTimeOffset RecordedAtUtc { get; set; }
}

public record IngestSalesLocationBatchResponse
{
    public int AcceptedCount { get; set; }
}
