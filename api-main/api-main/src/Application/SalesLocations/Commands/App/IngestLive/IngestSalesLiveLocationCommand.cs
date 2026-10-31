namespace Application.SalesLocations.Commands.App.IngestLive;

public record IngestSalesLiveLocationCommand : IRequest<Result<IngestSalesLiveLocationResponse>>
{
    public int UserId { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public record IngestSalesLiveLocationDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public record IngestSalesLiveLocationResponse
{
    public int Id { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public DateTimeOffset RecordedAtUtc { get; set; }
}
