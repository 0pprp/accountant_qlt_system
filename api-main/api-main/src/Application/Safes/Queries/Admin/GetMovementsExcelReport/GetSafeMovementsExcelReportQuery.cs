using Application.Safes.Queries.Admin.GetMovements;

namespace Application.Safes.Queries.Admin.GetMovementsExcelReport;

public record GetSafeMovementsExcelReportQuery : IRequest<Result<GetSafeMovementsExcelReportResponse>>
{
    public int SafeId { get; set; }
    public required GetSafeMovementsFilter Filter { get; set; }
}

public record GetSafeMovementsExcelReportResponse
{
    public required byte[] Data { get; set; }
    public required string ContentType { get; set; }
    public required string FileName { get; set; }
}

public record GetSafeMovementExcelReportItem
{
    public int Id { get; set; }
    public required string Kind { get; set; }
    public double Amount { get; set; }
    public double BalanceAfter { get; set; }
    public string? CounterpartySafeName { get; set; }
    public string? Note { get; set; }
    public string? PerformedByFullName { get; set; }
    public string? CreatedAt { get; set; }
}
