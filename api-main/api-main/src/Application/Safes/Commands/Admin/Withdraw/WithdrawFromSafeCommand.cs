namespace Application.Safes.Commands.Admin.Withdraw;

public record WithdrawFromSafeCommand : IRequest<Result>
{
    public int SafeId { get; set; }
    public double Amount { get; set; }
    public string? Note { get; set; }
    public Guid? OperationKey { get; set; }
}
