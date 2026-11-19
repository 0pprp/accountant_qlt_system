using System.Data;
using Application.Archive.Common;
using Application.Customers.Common;
using Application.Common.Interfaces;
using Application.Orders.Commands.Admin.ChangeApprovalStatus;
using Application.Orders.Common;
using Application.SalesEmployees.Common;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.AttachmentAggregate.Enums;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.App.Checkout;

public record ListChoice(int Id, string Name, string MandobFullName);
public record CheckoutLists(int OrderId, int? RequestId = null) : IRequest<Result<List<ListChoice>>>;
public record SaleLine(int Id, int Qty);
public record CheckoutBody(int OrderListId, List<SaleLine> Products, decimal Total,
    decimal DailyInstallment, decimal Prepayment, string Address, double Lat, double Lng,
    string? ShopName = null, string? BusinessType = null, double? GoodsValue = null, double? DailyIncome = null,
    double? ShopLength = null, double? ShopWidth = null, string? Note = null);
public record CheckoutSale(int OrderId, CheckoutBody Body) : IRequest<Result<int>>;

public class RequestCheckout(IApplicationDbContext db, ICurrentUserService user,
    IDateTimeProvider clock, IMediator mediator) :
    IRequestHandler<CheckoutLists, Result<List<ListChoice>>>, IRequestHandler<CheckoutSale, Result<int>>
{
    private IQueryable<Domain.Entities.SalesRequestAggregate.SalesRequest> Owned(int orderId, int? requestId = null)
    {
        var branches = user.BranchIds ?? [];
        return db.SalesRequests.Where(r => (requestId.HasValue ? r.Id == requestId : r.LinkedOrderId == orderId) && !r.IsDeleted &&
            r.TargetEmployeeId == user.UserId && branches.Contains(r.BranchId));
    }

    public async Task<Result<List<ListChoice>>> Handle(CheckoutLists query, CancellationToken ct)
    {
        var branch = await Owned(query.OrderId, query.RequestId).Select(r => (int?)r.BranchId).SingleOrDefaultAsync(ct);
        if (branch is null || user.UserId is not int actor ||
            !await SalesWorkforce.HasActiveAssignmentAsync(db, actor, branch.Value, clock.UtcNowOffset, ct))
            return SalesRequestErrors.NotOwner;
        return await db.OrderLists.AsNoTracking().Where(l => l.BranchId == branch)
            .OrderBy(l => l.Name).ThenBy(l => l.Id)
            .Select(l => new ListChoice(l.Id, l.Name, l.Mandob!.FullName)).ToListAsync(ct);
    }

    public async Task<Result<int>> Handle(CheckoutSale cmd, CancellationToken ct)
    {
        try
        {
            return await CheckoutAsync(cmd, ct);
        }
        catch (Exception e) when (e is DbUpdateConcurrencyException
                                  || e.GetBaseException() is System.Data.Common.DbException { SqlState: "40001" })
        {
            return SalesRequestErrors.ConcurrencyConflict;
        }
    }

    private async Task<Result<int>> CheckoutAsync(CheckoutSale cmd, CancellationToken ct)
    {
        var b = cmd.Body;
        if (user.UserId is not int actor) return SalesRequestErrors.NotOwner;
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var request = await Owned(cmd.OrderId).SingleOrDefaultAsync(ct);
        if (request == null || !await SalesWorkforce.HasActiveAssignmentAsync(db, actor, request.BranchId, clock.UtcNowOffset, ct))
            return SalesRequestErrors.NotOwner;
        var order = await db.Orders.Include(o => o.OrderItems).Include(o => o.Attachments).Include(o => o.InstallmentPayments)
            .SingleOrDefaultAsync(o => o.Id == cmd.OrderId && o.BranchId == request.BranchId && o.SellerId == actor, ct);
        if (order == null) return OrderErrors.OrderNotFound;
        if (request.Status == SalesRequestStatus.Completed && order.ApprovalStatus == OrderApprovalStatus.Approved)
        {
            var sameLines = b.Products != null && order.OrderItems.Count == b.Products.Count &&
                order.OrderItems.All(i => b.Products.Any(l => l.Id == i.ProductId && l.Qty == i.Quantity));
            if (!sameLines || order.OrderListId != b.OrderListId ||
                Math.Abs(order.GrossSell() - (double)b.Total) > 0.005 ||
                Math.Abs(order.PrepaymentAmount - (double)b.Prepayment) > 0.005 ||
                Math.Abs(order.GrossDaily() - (double)b.DailyInstallment) > 0.005)
                return SalesRequestErrors.ConcurrencyConflict;
            return order.Id;
        }
        if (request.Status is not (SalesRequestStatus.ConvertedToSale or SalesRequestStatus.Inspected) || order.ApprovalStatus != OrderApprovalStatus.Pending)
            return SalesRequestErrors.IllegalTransition;
        if (!await db.OrderLists.AnyAsync(l => l.Id == b.OrderListId && l.BranchId == order.BranchId, ct))
            return OrderErrors.TheSellerDoesNotBelongToThisOrderList;
        // وصل الأمانة وعقد البيع اختياريان؛ وصل الشراء لم يعد مطلوباً.
        if (b.Products is not {Count: > 0 and <= 100} || b.Products.Any(p => p.Qty is < 1 or > 100000) ||
            b.Products.Select(p => p.Id).Distinct().Count() != b.Products.Count ||
            b.Total <= 0 || b.Prepayment < 0 || b.Prepayment > b.Total || b.DailyInstallment <= 0 ||
            string.IsNullOrWhiteSpace(b.Address) || b.Address.Length > 300 || !double.IsFinite(b.Lat) ||
            !double.IsFinite(b.Lng) || Math.Abs(b.Lat)>90 || Math.Abs(b.Lng)>180)
            return new Error("تحقق من المواد والأسعار وبيانات البيع", "Sale_InvalidCheckout");
        var ids = b.Products.Select(p => p.Id).ToList();
        var catalog = await db.Products.Where(p => ids.Contains(p.Id) && p.Warehouse!.BranchId == order.BranchId)
            .ToDictionaryAsync(p => p.Id, ct);
        if (catalog.Count != ids.Count) return OrderErrors.SomeSelectedProductsNotFound;
        var retail = b.Products.Sum(l => (decimal)catalog[l.Id].SellAmount * l.Qty);
        if (retail <= 0 || catalog.Values.Any(p => p.SellAmount <= 0))
            return new Error("سعر المادة غير صالح", "Sale_InvalidCatalogPrice");
        if (order.OrderItems.Count != 0) return SalesRequestErrors.IllegalTransition;
        decimal weight = 0, sold = 0, paid = 0, daily = 0;
        foreach (var line in b.Products)
        {
            var p = catalog[line.Id];
            if (p.RemainingCount < line.Qty) return OrderErrors.ThereIsNotEnoughQuantityOfThisProduct;
            weight += (decimal)p.SellAmount * line.Qty;
            decimal Portion(decimal amount) => weight == retail ? amount : decimal.Round(amount * weight / retail, 2);
            var sell = Portion(b.Total); var pre = Portion(b.Prepayment); var rate = Portion(b.DailyInstallment);
            order.OrderItems.Add(new OrderItem(ProductType.Warehouse, line.Qty, p.BuyAmount * line.Qty,
                (double)(sell-sold), (double)(pre-paid), (double)(rate-daily), p.Name, p.Id));
            sold = sell; paid = pre; daily = rate;
        }
        order.BuyAmount = order.OrderItems.Sum(i => i.BuyAmount);
        var buyer = await db.Customers.SingleOrDefaultAsync(c => c.Id == order.CustomerId, ct);
        if (buyer == null) return CustomerErrors.CustomerNotFound;
        order.SetSaleAmounts((double)b.Total, (double)b.Prepayment, (double)b.DailyInstallment,
            buyer.SellAmountDiscount, buyer.DailyInstallmentDiscount);
        var discounted = CustomerDiscountApplier.Plan([order], buyer.SellAmountDiscount, buyer.DailyInstallmentDiscount);
        if (discounted.IsFailed) return discounted.Error!;
        var now = clock.UtcNowOffset;
        var baghdad = now.ToOffset(TimeSpan.FromHours(3)).DateTime;
        order.SetSeller(actor, b.OrderListId, new Location(b.Lat,b.Lng), b.Address.Trim(),
            DateOnly.FromDateTime(baghdad), TimeOnly.FromDateTime(baghdad));
        db.CustomerShopProfiles.Add(ShopProfile(order, b, now));
        await db.SaveChangesAsync(ct);
        var approval = await mediator.Send(new ChangeOrderApprovalStatusCommand {
            OrderId = order.Id, ApprovalStatus = OrderApprovalStatus.Approved }, ct);
        if (approval.IsFailed) return approval.Error!;
        var previous = request.Status;
        request.CompleteFromSale(now);
        SalesRequestTimelineWriter.Append(db, request.Id, SalesRequestTimelineEventType.Completed, actor,
            now, previous, request.Status, actor, actor, null, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(db, request, SalesRequestChangeType.Completed, now);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // خارج معاملة الـSerializable عمداً: تسابق حزمتين على نفس الصفّ كان سيُرجِع
        // البيع كلّه. الأرشيف تفصيل محاسبي، والمُصالِح الدوري يلتقط ما يفوت هنا.
        await TryEnterArchiveAsync(order, ct);

        return order.Id;
    }

    private static CustomerShopProfile ShopProfile(Order order, CheckoutBody b, DateTimeOffset now)
    {
        static double? Positive(double? v) => v is > 0 && double.IsFinite(v.Value) ? v : null;
        static string? Text(string? v, int max) => string.IsNullOrWhiteSpace(v) ? null : v.Trim()[..Math.Min(v.Trim().Length, max)];

        var length = Positive(b.ShopLength);
        var width = Positive(b.ShopWidth);
        var photo = order.Attachments
            .Where(a => a.Type == AttachmentType.CustomerShopPhoto)
            .OrderByDescending(a => a.Id)
            .Select(a => a.RelativePath)
            .FirstOrDefault();
        var businessType = string.Join(" — ", new[] { b.BusinessType, b.Note }
            .Where(v => string.IsNullOrWhiteSpace(v) == false)
            .Select(v => v!.Trim()));
        var imageKey = photo is not null && photo.Length <= CustomerDocument.MaxKeyLength ? photo : null;

        return new CustomerShopProfile(
            order.CustomerId,
            order.Id,
            Text(b.ShopName, CustomerShopProfile.MaxNameLength),
            Text(businessType, CustomerShopProfile.MaxNameLength),
            Positive(b.GoodsValue),
            Positive(b.DailyIncome),
            length,
            width,
            length is not null && width is not null ? length * width : null,
            imageKey,
            now);
    }

    private async Task TryEnterArchiveAsync(Order order, CancellationToken ct)
    {
        try
        {
            var transaction = await ArchiveIntake.EnsureAsync(db, order, ct);
            if (transaction is not null)
                await db.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            // صامت بقصد — البيع تمّ، والمُصالِح سيُدخِله لاحقاً.
        }
    }
}
