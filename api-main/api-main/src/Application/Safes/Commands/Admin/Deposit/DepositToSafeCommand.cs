namespace Application.Safes.Commands.Admin.Deposit;

public record DepositToSafeCommand : IRequest<Result>
{
    public int SafeId { get; set; }
    public double Amount { get; set; }
    public string? Note { get; set; }

    /// <summary>مفتاح منع التكرار — يُولَّد مرة واحدة عند فتح النافذة في اللوحة.</summary>
    public Guid? OperationKey { get; set; }
}
