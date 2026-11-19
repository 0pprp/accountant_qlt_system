namespace Domain.Entities.ArchiveAggregate.Enums;

/// <summary>
/// من أين جاء التاريخ الذي بُوِّبت به المعاملة. يُخزَّن لأن <c>Order.SaleDate</c> ليس مضموناً:
/// <c>Order.CompleteSalesWorkflow</c> لا يضعه، و<c>SetSeller</c> وحده يضعه.
/// </summary>
public enum ArchivePeriodSource : byte
{
    SaleDate = 0,
    SalesCompletedAt = 1,
    OrderCreatedAt = 2
}
