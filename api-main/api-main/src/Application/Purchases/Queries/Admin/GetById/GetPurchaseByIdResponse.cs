using Application.Attachments.Common;
using Application.Purchases.Queries.Admin.GetPaginated;
using Domain.Entities.SafeAggregate.Enums;

namespace Application.Purchases.Queries.Admin.GetById;

public record GetPurchaseByIdResponse
{
    public int Id { get; set; }
    public int FactorNumber { get; set; }
    public double TotalAmount { get; set; }
    public SafeType SafeType { get; set; }
    public int PurchaseItemsCount { get; set; }
    public required List<PurchaseItemDto> PurchaseItems { get; set; }
    public required List<GetAttachmentDto> Attachments { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}