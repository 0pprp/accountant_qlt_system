using Application.Attachments.Common;
using Application.Common.Interfaces;
using Application.Products.Common;
using Application.Purchases.Common;
using Application.Purchases.Queries.Admin.GetPaginated;
using Microsoft.EntityFrameworkCore;

namespace Application.Purchases.Queries.Admin.GetById;

public class GetPurchaseByIdHandler : IRequestHandler<GetPurchaseByIdQuery, Result<GetPurchaseByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPurchaseByIdHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<GetPurchaseByIdResponse>> Handle(GetPurchaseByIdQuery request, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases.AsNoTracking()
            .Select(x => new GetPurchaseByIdResponse
            {
                Id = x.Id,
                FactorNumber = x.FactorNumber,
                TotalAmount = x.TotalAmount,
                SafeType = x.SafeType,
                PurchaseItemsCount = x.PurchaseItems.Count,
                PurchaseItems = x.PurchaseItems
                    .Select(pi => new PurchaseItemDto
                    {
                        Id = pi.Id,
                        Quantity = pi.Quantity,
                        Amount = pi.Amount,
                        Product = pi.ProductId != null
                            ? new ProductDto
                            {
                                Id = pi.ProductId.Value,
                                Name = pi.Product!.Name
                            }
                            : null,
                        ForeignProductName = pi.ForeignProductName
                    })
                    .ToList(),
                Attachments = x.Attachments
                    .Select(a => new GetAttachmentDto
                    {
                        Id = a.Id,
                        OriginalFileName = a.OriginalFileName,
                        RelativePath = a.RelativePath,
                        Type = a.Type,
                        FileSizeInByte = a.FileSizeInByte
                    })
                    .ToList(),
                CreatedAt = x.CreatedAt!.Value,
            })
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (purchase is null)
            return PurchaseErrors.PurchaseNotFound;

        return purchase;
    }
}