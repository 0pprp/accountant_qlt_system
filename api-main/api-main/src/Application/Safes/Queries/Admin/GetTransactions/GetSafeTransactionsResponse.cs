using Domain.Entities.SafeAggregate.Enums;

namespace Application.Safes.Queries.Admin.GetTransactions;

public record GetSafeTransactionsResponse
{
    public int Id { get; set; }
    public required string Source { get; set; }
    public required string Destination { get; set; }
    public double Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public string? StatusDescription { get; set; }
    public TransactionDirection Direction { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}