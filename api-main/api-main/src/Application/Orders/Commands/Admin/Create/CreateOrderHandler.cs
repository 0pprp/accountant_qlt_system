using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.Customers.Common;
using Application.Orders.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.AttachmentAggregate;
using Domain.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands.Admin.Create;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileManager _fileManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public CreateOrderHandler(IApplicationDbContext dbContext, IFileManager fileManager, ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _fileManager = fileManager;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == request.CustomerId, cancellationToken);
        if (customer is null)
            return CustomerErrors.CustomerNotFound;

        if (_currentUserService.BranchIds!.Contains(customer.BranchId) == false)
            return OrderErrors.CustomerDoesNotBelongToYourBranch;

        var decreaseProductsInventoryResult = await DecreaseProductsInventoryAsync(request, cancellationToken);
        if (decreaseProductsInventoryResult.IsFailed)
            return decreaseProductsInventoryResult.Error!;

        var totalBuyAmount = request.OrderItems.Sum(x => x.BuyAmount);
        var totalSellAmount = request.OrderItems.Sum(x => x.SellAmount);
        var totalPrepaymentAmount = request.OrderItems.Sum(x => x.PrepaymentAmount);
        var totalDailyInstallmentAmount = request.OrderItems.Sum(x => x.DailyInstallmentAmount);

        var orderItems = request.OrderItems
            .Select(x => new OrderItem(
                x.ProductType,
                x.Quantity,
                x.BuyAmount,
                x.SellAmount,
                x.PrepaymentAmount,
                x.DailyInstallmentAmount,
                x.ProductName,
                x.ProductId))
            .ToList();

        var order = Order.CreateByAdmin(
            totalBuyAmount,
            totalSellAmount,
            totalPrepaymentAmount,
            totalDailyInstallmentAmount,
            request.CustomerId,
            customer.BranchId,
            orderItems);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.AddAsync(
            customer.BranchId,
            ActivityType.OrderCreated,
            TargetEntityType.Order,
            order.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await SaveAttachmentsAsync(request.Attachments, order.Id);

        return new CreateOrderResponse(order.Id);
    }

    private async Task<Result> DecreaseProductsInventoryAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.OrderItems
            .Where(x => x.ProductId is not null)
            .Select(x => x.ProductId).ToList();

        var products = await _dbContext.Products
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        if (products.Count != productIds.Count)
            return OrderErrors.SomeSelectedProductsNotFound;

        foreach (var product in products)
        {
            var orderItem = request.OrderItems.First(x => x.ProductId == product.Id);
            if (orderItem.Quantity > product.RemainingCount)
                return OrderErrors.ThereIsNotEnoughQuantityOfThisProduct;

            product.RemainingCount -= orderItem.Quantity;
        }

        return Result.Success();
    }

    private async Task SaveAttachmentsAsync(List<CreateOrderAttachmentDto> attachments, int orderId)
    {
        foreach (var attachmentDto in attachments)
        {
            var fileDto = await _fileManager.SaveFileAsync(attachmentDto.File, Path.Combine(AttachmentPath.Root,
                AttachmentPath.OrdersSubfolder(orderId)));

            var attachment = new Attachment(attachmentDto.File.FileName, fileDto.Name, fileDto.RelativePath, attachmentDto.File.ContentType,
                fileDto.Extension, attachmentDto.File.Length, attachmentDto.Type,
                AttachmentUtility.GetFileType(attachmentDto.File.ContentType))
            {
                OrderId = orderId
            };

            _dbContext.Attachments.Add(attachment);
        }

        await _dbContext.SaveChangesAsync();
    }
}