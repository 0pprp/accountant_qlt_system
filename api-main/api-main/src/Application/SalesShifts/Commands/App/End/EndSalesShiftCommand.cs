namespace Application.SalesShifts.Commands.App.End;

public record EndSalesShiftCommand(int UserId) : IRequest<Result<EndSalesShiftResponse>>;

public record EndSalesShiftResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset EndedAtUtc { get; set; }
    public bool HasActiveShift { get; set; }
}
