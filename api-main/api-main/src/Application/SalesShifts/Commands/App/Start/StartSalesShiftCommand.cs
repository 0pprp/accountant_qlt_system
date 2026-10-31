namespace Application.SalesShifts.Commands.App.Start;

public record StartSalesShiftCommand(int UserId) : IRequest<Result<StartSalesShiftResponse>>;

public record StartSalesShiftResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public bool HasActiveShift { get; set; } = true;
}
