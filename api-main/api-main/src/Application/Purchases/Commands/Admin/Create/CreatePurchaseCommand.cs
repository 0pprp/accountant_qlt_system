using Domain.Entities.SafeAggregate.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Purchases.Commands.Admin.Create;

public record CreatePurchaseCommand : IRequest<Result>
{
    public required int FactorNumber { get; set; }
    public required SafeType SafeType { get; set; }
    public int? BranchId { get; set; }
    public required List<PurchaseItemDto> PurchaseItems { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}

public record PurchaseItemDto
{
    public required int ProductId { get; set; }
    public required int Quantity { get; set; }
    public required double Amount { get; set; }
}