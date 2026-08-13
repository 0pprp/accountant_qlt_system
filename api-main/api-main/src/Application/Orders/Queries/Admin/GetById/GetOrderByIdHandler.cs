using Application.Attachments.Common;
using Application.Common.Interfaces;
using Application.Orders.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.Admin.GetById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOrderByIdHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetOrderByIdResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetOrderByIdResponse
            {
                OrderInfo = new OrderInfoDto
                {
                    Id = x.Id,
                    OrderItems = x.OrderItems
                        .Select(oi => new OrderItemDto
                        {
                            Id = oi.Id,
                            ProductType = oi.ProductType,
                            Quantity = oi.Quantity,
                            BuyAmount = oi.BuyAmount,
                            SellAmount = oi.SellAmount,
                            PrepaymentAmount = oi.PrepaymentAmount,
                            DailyInstallmentAmount = oi.DailyInstallmentAmount,
                            ProductId = oi.ProductId,
                            ProductName = oi.ProductName ?? oi.Product!.Name
                        })
                        .ToList(),
                    BuyAmount = x.BuyAmount,
                    SellAmount = x.SellAmount,
                    PrepaymentAmount = x.PrepaymentAmount,
                    DailyInstallmentAmount = x.DailyInstallmentAmount,
                    Attachments = x.Attachments.Select(a => new GetAttachmentDto
                    {
                        Id = a.Id,
                        OriginalFileName = a.OriginalFileName,
                        RelativePath = a.RelativePath,
                        FileSizeInByte = a.FileSizeInByte,
                        Type = a.Type
                    }).ToList()
                },
                SellerInfo = x.SellerId != null
                    ? new SellerInfoDto
                    {
                        Seller = new SellerDto
                        {
                            Id = x.SellerId!.Value,
                            Name = x.Seller!.FullName
                        },
                        OrderList = new OrderListDto
                        {
                            Id = x.OrderListId!.Value,
                            Name = x.OrderList!.Name
                        },
                        CreationAddress = x.CreationAddress,
                        SaleDate = x.SaleDate!.Value,
                        SaleTime = x.SaleTime!.Value
                    }
                    : null
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        return order;
    }
}