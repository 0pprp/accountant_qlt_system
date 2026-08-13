using Domain.Common;
using Domain.Entities.SafeAggregate.Enums;

namespace Domain.Entities.SafeAggregate;

public class SafeTransferTransaction : AuditableEntity
{
    public SafeTransferTransaction(string? description, int? sourceSafeId, int destinationSafeId)
    {
        Description = description;
        SourceSafeId = sourceSafeId;
        DestinationSafeId = destinationSafeId;
    }

    public string? Description { get; set; }
    public int TransactionId { get; set; }
    public int? SourceSafeId { get; set; }
    public int DestinationSafeId { get; set; }

    public Transaction? Transaction { get; set; }
    public Safe? SourceSafe { get; set; }
    public Safe? DestinationSafe { get; set; }
}