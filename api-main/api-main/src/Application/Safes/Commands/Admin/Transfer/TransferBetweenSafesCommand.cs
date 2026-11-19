namespace Application.Safes.Commands.Admin.Transfer;

public record TransferBetweenSafesCommand : IRequest<Result>
{
    public int SourceSafeId { get; set; }
    public int DestinationSafeId { get; set; }
    public double Amount { get; set; }
    public string? Note { get; set; }
    public Guid? OperationKey { get; set; }
}
