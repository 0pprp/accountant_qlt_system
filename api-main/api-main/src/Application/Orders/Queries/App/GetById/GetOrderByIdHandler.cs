using Application.Attachments.Common;
using Application.Common.Interfaces;
using Application.Orders.Common;
using Application.Orders.Queries.Admin.GetById;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.App.GetById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetOrderByIdHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetOrderByIdResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var order = await _dbContext.Orders.AsNoTracking()
            .Where(x => x.SellerId == request.UserId || x.OrderList!.MandobId == request.UserId || x.OrderList.MotabaId == request.UserId)
            .Select(x => new GetOrderByIdResponse
            {
                Id = x.Id,
                BuyAmount = x.BuyAmount,
                SellAmount = x.SellAmount,
                PrepaymentAmount = x.PrepaymentAmount,
                DailyInstallmentAmount = x.DailyInstallmentAmount,
                PaidAmount = x.InstallmentPayments.Sum(ip => ip.Amount),
                // Overdue Amount = |Expected - Paid|
                OverdueAmount = x.ExecutionStatus == OrderExecutionStatus.Completed || x.ExecutionStatus == OrderExecutionStatus.NotStarted
                    ? 0
                    : x.DailyInstallmentAmount *
                      Math.Max(0, today.DayNumber - (x.SaleDate!.Value.DayNumber + 1)) -
                      x.InstallmentPayments
                          // Excluding the first installment (Prepayment)
                          .Where(ip => ip.Date != x.SaleDate)
                          .Where(ip => ip.Date != today)
                          .Sum(ip => ip.Amount),
                Location = x.Location,
                CreationAddress = x.CreationAddress,
                SaleDate = x.SaleDate,
                SaleTime = x.SaleTime,
                Step = x.Step,
                ExecutionStatus = x.ExecutionStatus,
                ApprovalStatus = x.ApprovalStatus,
                Seller = new SellerDto
                {
                    Id = x.SellerId!.Value,
                    Name = x.Seller!.FullName
                },
                OrderList = x.OrderListId != null
                    ? new OrderListDto
                    {
                        Id = x.OrderListId!.Value,
                        Name = x.OrderList!.Name
                    }
                    : null,
                Attachments = x.Attachments.Select(a => new GetAttachmentDto
                {
                    Id = a.Id,
                    OriginalFileName = a.OriginalFileName,
                    RelativePath = a.RelativePath,
                    FileSizeInByte = a.FileSizeInByte,
                    Type = a.Type
                }).ToList(),
                OrderItems = x.OrderItems
                    .Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName ?? oi.Product!.Name,
                        ProductType = oi.ProductType,
                        Quantity = oi.Quantity,
                        BuyAmount = oi.BuyAmount,
                        SellAmount = oi.SellAmount,
                        PrepaymentAmount = oi.PrepaymentAmount,
                        DailyInstallmentAmount = oi.DailyInstallmentAmount
                    })
                    .ToList(),
                CreatedAt = x.CreatedAt!.Value,
            })
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
        if (order is null)
            return OrderErrors.OrderNotFound;

        return order;
    }
}