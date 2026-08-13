namespace Application.Safes.Commands.Admin.CreateSafeTransfer;

public record CreateSafeTransferCommand : IRequest<Result>
{
    public double Amount { get; set; }
    public int? SourceSafeId { get; set; }
    public int DestinationSafeId { get; set; }
    public string? Description { get; set; }
}