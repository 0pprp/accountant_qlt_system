using Domain.Entities.SafeAggregate.Enums;

namespace Application.Transactions.Commands.Admin.ChangeStatus;

public record ChangeTransactionStatusCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required TransactionStatus Status { get; set; }
    public string? StatusDescription { get; set; }
}

public record ChangeTransactionStatusDto
{
    public required TransactionStatus Status { get; set; }
    public string? StatusDescription { get; set; }
}