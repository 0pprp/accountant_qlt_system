namespace Application.SalesShifts.Queries.App.GetCurrent;

public record GetCurrentSalesShiftQuery(int UserId) : IRequest<Result<GetCurrentSalesShiftResponse>>;

public record GetCurrentSalesShiftResponse
{
    public bool HasActiveShift { get; set; }
    public int? Id { get; set; }
    public int? UserId { get; set; }
    public int? BranchId { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
}
