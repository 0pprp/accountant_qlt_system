using System.Text.Encodings.Web;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Orders.Common;
using Application.Safes.Common;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.SafeAggregate.Enums;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.DeleteSoldSale;

public static class DeletedSoldSaleLog
{
    public const string Marker = "sold-sale-deleted";

    private static readonly JsonSerializerOptions Options = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static string Details(DeleteSoldSaleImpact impact, string reason)
    {
        var json = JsonSerializer.Serialize(new
        {
            kind = Marker,
            r = impact.RequestId,
            o = impact.OrderId,
            c = Clip(impact.CustomerName, 80),
            p = impact.CustomerPhone,
            d = impact.SaleDate?.ToString("yyyy-MM-dd"),
            s = impact.SellAmount,
            pp = impact.PrepaymentAmount,
            paid = impact.PaidTotal,
            n = impact.PaymentsCount,
            fr = impact.ForeignPurchaseRefund,
            why = Clip(reason, 200),
            items = Clip(string.Join("، ", impact.Items.Select(i => $"{i.Name}×{i.Quantity}")), 300)
        }, Options);
        return json;
    }

    private static string Clip(string value, int max) => value.Length <= max ? value : value[..max] + "…";
}

public class DeleteSoldSaleHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    ISafeLedger safeLedger,
    IActivityLogService activityLog)
    : IRequestHandler<DeleteSoldSaleCommand, Result<DeleteSoldSaleImpact>>,
      IRequestHandler<GetDeleteSoldSaleImpactQuery, Result<DeleteSoldSaleImpact>>
{
    public async Task<Result<DeleteSoldSaleImpact>> Handle(GetDeleteSoldSaleImpactQuery request, CancellationToken cancellationToken)
    {
        var loaded = await LoadAsync(request.RequestId, tracking: false, cancellationToken);
        if (loaded.IsFailed)
            return loaded.Error!.Value;
        return await BuildImpactAsync(loaded.Data!.Request, loaded.Data.Order, cancellationToken);
    }

    public async Task<Result<DeleteSoldSaleImpact>> Handle(DeleteSoldSaleCommand request, CancellationToken cancellationToken)
    {
        var loaded = await LoadAsync(request.RequestId, tracking: true, cancellationToken);
        if (loaded.IsFailed)
            return loaded.Error!.Value;

        var (salesRequest, order) = loaded.Data!;
        var impact = await BuildImpactAsync(salesRequest, order, cancellationToken);
        if (impact.PaymentsCount > 0 && request.ConfirmPayments == false)
            return SalesRequestErrors.DeleteSoldSalePaymentsNotConfirmed;

        var reason = request.Reason.Trim();
        var at = clock.UtcNowOffset;
        var actorId = currentUser.UserId ?? 0;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        foreach (var item in order.OrderItems.Where(i => i.ProductId != null && i.Product != null))
            item.Product!.RemainingCount += item.Quantity;

        var paidTotal = order.InstallmentPayments.Sum(p => p.Amount);
        if (paidTotal > 0)
        {
            var seller = await OrderCashHolder.GetAsync(db, order.SellerId, cancellationToken);
            if (seller.IsSucceeded)
                seller.Data!.UndeliveredCashAmount -= paidTotal;
        }

        var purchases = await db.Purchases.Where(p => p.OrderId == order.Id).ToListAsync(cancellationToken);
        var refund = purchases.Sum(p => p.TotalAmount);
        if (refund > 0)
        {
            var branchSafe = await safeLedger.GetBranchSafeAsync(order.BranchId, cancellationToken);
            if (branchSafe.IsFailed)
                return branchSafe.Error!.Value;
            var refunded = await safeLedger.RecordAsync(new SafeLedgerEntry
            {
                SafeId = branchSafe.Data!.Id,
                Kind = SafeMovementKind.Refund,
                Amount = refund,
                Note = $"إرجاع شراء منتجات خارجية لمبيع محذوف {order.Id}",
                PerformedByUserId = actorId,
                OrderId = order.Id
            }, cancellationToken);
            if (refunded.IsFailed)
                return refunded.Error!.Value;
        }

        var archiveRows = await db.ArchiveTransactions
            .Include(a => a.Events)
            .Where(a => a.OrderId == order.Id)
            .ToListAsync(cancellationToken);
        foreach (var archive in archiveRows)
        {
            db.ArchiveTransactionEvents.RemoveRange(archive.Events);
            db.ArchiveTransactions.Remove(archive);
        }

        var tasks = await db.ManagementTasks.Where(t => t.OrderId == order.Id).ToListAsync(cancellationToken);
        foreach (var task in tasks)
            db.Entry(task).Property(nameof(ManagementTask.OrderId)).CurrentValue = null;

        var prevEmployee = salesRequest.TargetEmployeeId;
        salesRequest.LinkedOrderId = null;
        salesRequest.SoftDelete(reason, actorId, at);

        var reversed = await new PaymentSafePosting(db, safeLedger).ReverseManyAsync(
            order.InstallmentPayments.Select(p => p.Id), actorId, cancellationToken);
        if (reversed.IsFailed)
            return reversed.Error!.Value;

        db.InstallmentPayments.RemoveRange(order.InstallmentPayments);
        db.Purchases.RemoveRange(purchases);
        db.Orders.Remove(order);

        SalesRequestTimelineWriter.Append(
            db, salesRequest.Id, SalesRequestTimelineEventType.SoldSaleDeleted, actorId, at,
            salesRequest.Status, salesRequest.Status, prevEmployee, prevEmployee, reason, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(db, salesRequest, SalesRequestChangeType.Invalidation, at, prevEmployee);
        await activityLog.AddAsync(
            order.BranchId, ActivityType.OrderDeleted, TargetEntityType.Order, order.Id,
            details: DeletedSoldSaleLog.Details(impact, reason));

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return impact;
    }

    private async Task<Result<(SalesRequest Request, Order Order)>> LoadAsync(int requestId, bool tracking, CancellationToken ct)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        var requests = tracking ? db.SalesRequests : db.SalesRequests.AsNoTracking();
        var salesRequest = await requests.FirstOrDefaultAsync(x => x.Id == requestId && x.IsDeleted == false, ct);
        if (salesRequest is null || branches.Contains(salesRequest.BranchId) == false)
            return SalesRequestErrors.NotFound;
        if (salesRequest.Status != SalesRequestStatus.Completed || salesRequest.LinkedOrderId is not int orderId)
            return SalesRequestErrors.SoldSaleRequired;

        var orders = tracking ? db.Orders : db.Orders.AsNoTracking();
        var order = await orders
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Include(o => o.InstallmentPayments)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null || branches.Contains(order.BranchId) == false)
            return SalesRequestErrors.SoldSaleRequired;

        return (salesRequest, order);
    }

    private async Task<DeleteSoldSaleImpact> BuildImpactAsync(SalesRequest salesRequest, Order order, CancellationToken ct)
    {
        var refund = await db.Purchases.AsNoTracking().Where(p => p.OrderId == order.Id).SumAsync(p => (double?)p.TotalAmount, ct) ?? 0;
        var archives = await db.ArchiveTransactions.AsNoTracking().CountAsync(a => a.OrderId == order.Id, ct);
        return new DeleteSoldSaleImpact(
            salesRequest.Id,
            order.Id,
            salesRequest.CustomerName,
            salesRequest.CustomerPhone,
            order.SaleDate,
            order.SellAmount,
            order.PrepaymentAmount,
            order.InstallmentPayments.Sum(p => p.Amount),
            order.InstallmentPayments.Count,
            refund,
            archives,
            order.OrderItems
                .Select(i => new DeleteSoldSaleImpactItem(
                    i.Product?.Name ?? i.ProductName ?? "مادة",
                    i.Quantity,
                    i.ProductId != null && i.Product != null))
                .ToList());
    }
}
