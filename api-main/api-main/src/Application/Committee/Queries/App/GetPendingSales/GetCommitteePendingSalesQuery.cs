namespace Application.Committee.Queries.App.GetPendingSales;

public record GetCommitteePendingSalesQuery(int UserId)
    : IRequest<Result<List<CommitteePendingSaleDto>>>;

public record CommitteePendingSaleDto
{
    public int OrderId { get; set; }
    public int? ListId { get; set; }
    public required string CustomerName { get; set; }
    public double SellAmount { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}
