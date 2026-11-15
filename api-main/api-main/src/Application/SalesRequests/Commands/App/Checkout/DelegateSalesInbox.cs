using System.Data;
using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.WrongSales.Commands;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.App.Checkout;

public class DelegateSale
{
    public int OrderId { get; init; }
    public int RequestId { get; init; }
    public int DelegateId { get; init; }
    public double DailyInstallment { get; init; }
    public int CustomerId { get; init; }
    public int ListId { get; init; }
    public required string Customer { get; init; }
    public required string ListName { get; init; }
    public double Amount { get; init; }
    public DateOnly? SaleDate { get; init; }
}
public record DueDelegateSales(int AfterId) : IRequest<Result<List<DelegateSale>>>;
public record EvaluateDelegateSale(int OrderId, int Rating, string? Note, decimal ShopStockEstimatedValue, decimal EstimatedDailyRevenue) : IRequest<Result<int>>;

public class DelegateSalesInbox(IApplicationDbContext db, IDateTimeProvider clock, ICurrentUserService user, IMediator mediator) :
    IRequestHandler<DueDelegateSales, Result<List<DelegateSale>>>, IRequestHandler<EvaluateDelegateSale, Result<int>>
{
    private IQueryable<DelegateSale> Due()
    {
        var start = new DateTimeOffset(clock.UtcNow.Date, TimeSpan.Zero); // 03:00 Baghdad
        return from r in db.SalesRequests
               join o in db.Orders on r.LinkedOrderId equals o.Id
               where !r.IsDeleted && r.Status == SalesRequestStatus.Completed &&
                    o.ApprovalStatus == OrderApprovalStatus.Approved && o.SalesCompletedAtUtc < start &&
                    o.OrderList!.MandobId == user.UserId && o.OrderList.BranchId == o.BranchId
               select new DelegateSale { OrderId=o.Id, RequestId=r.Id, CustomerId=o.CustomerId, ListId=o.OrderListId!.Value,
                    DelegateId=o.OrderList.MandobId, DailyInstallment=o.DailyInstallmentAmount, Customer=o.Customer!.FullName, ListName=o.OrderList.Name, Amount=o.SellAmount, SaleDate=o.SaleDate };
    }

    public async Task<Result<List<DelegateSale>>> Handle(DueDelegateSales q, CancellationToken ct)
        => await Due().Where(x => x.OrderId > q.AfterId && !db.InstallmentPayments.Any(p =>
            p.OrderId == x.OrderId && p.ClientPaymentId != null))
            .OrderBy(x => x.OrderId).Take(50).ToListAsync(ct);

    public async Task<Result<int>> Handle(EvaluateDelegateSale q, CancellationToken ct)
    {
        if (user.UserId is not int actor) return SalesRequestErrors.NotOwner;
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var sale = await Due().SingleOrDefaultAsync(x => x.OrderId == q.OrderId, ct);
        if (sale is null) return SalesRequestErrors.NotFound;
        if (await db.SalesRequestTimelineEntries.AnyAsync(e => e.SalesRequestId == sale.RequestId &&
            e.EventType == SalesRequestTimelineEventType.DelegateSaleRated, ct)) return sale.OrderId;
        if (q.Note?.Length > 350) return new Error("الملاحظة طويلة", "Sale_NoteTooLong");
        var result = await mediator.Send(new ReportMandobFirstCollectionWrongSaleCommand {
            ExactOrderId = sale.OrderId, CustomerId = sale.CustomerId, ListId = sale.ListId,
            Rating = q.Rating, Note = q.Note, ShopStockEstimatedValue = q.ShopStockEstimatedValue,
            EstimatedDailyRevenue = q.EstimatedDailyRevenue, SaleAmount = (decimal)sale.Amount }, ct);
        if (result.IsFailed) return result.Error!;
        var detail = $"تقييم {q.Rating}/10 · {q.Note}";
        SalesRequestTimelineWriter.Append(db, sale.RequestId, SalesRequestTimelineEventType.DelegateSaleRated,
            actor, clock.UtcNowOffset, null, null, null, null, detail, Guid.NewGuid());
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return sale.OrderId;
    }
}
