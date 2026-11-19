using Application.Common.Interfaces;
using Application.DelegateFollowup.Common;
using Application.SalesRatings.Common;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.DelegateFollowup.Queries.Admin.GetSalesOrders;

public class GetDelegateFollowupSalesOrdersHandler(
    IApplicationDbContext db,
    ICurrentUserService actor,
    IDateTimeProvider clock)
    : IRequestHandler<GetDelegateFollowupSalesOrdersQuery, Result<DelegateSalesOrdersDto>>
{
    private static readonly TimeSpan IraqOffset = TimeSpan.FromHours(3);

    public async Task<Result<DelegateSalesOrdersDto>> Handle(
        GetDelegateFollowupSalesOrdersQuery request,
        CancellationToken ct)
    {
        var branchError = DelegateFollowupAccess.ValidateBranch(actor, request.BranchId);
        if (branchError is not null)
            return branchError.Value;

        var list = await db.OrderLists.AsNoTracking()
            .Where(l => l.Id == request.ListId && l.BranchId == request.BranchId)
            .Select(l => new { l.Id, l.BranchId, l.MandobId })
            .FirstOrDefaultAsync(ct);
        if (list is null)
            return DelegateFollowupErrors.ListNotFound;

        var rows = await db.SalesRequests.AsNoTracking()
            .Where(r => r.BranchId == list.BranchId
                        && r.SourceType == SalesRequestSourceType.Delegate
                        && ((r.LinkedOrder != null && r.LinkedOrder.OrderListId == list.Id)
                            || r.CreatedByUserId == list.MandobId))
            .Select(r => new RequestRow
            {
                Id = r.Id,
                CustomerName = r.CustomerName,
                CustomerPhone = r.CustomerPhone,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
                IsDeleted = r.IsDeleted,
                RejectReason = r.RejectReason,
                DeleteReason = r.DeleteReason,
                ExistingCustomerId = r.ExistingCustomerId,
                CreatedByUserId = r.CreatedByUserId,
                StatusChangedAtUtc = r.StatusChangedAtUtc,
                TargetEmployeeName = r.TargetEmployee != null ? r.TargetEmployee.FullName : null,
                LinkedOrderId = r.LinkedOrderId,
                LinkedOrderListId = r.LinkedOrder != null ? r.LinkedOrder.OrderListId : null,
                OrderCustomerId = r.LinkedOrder != null ? r.LinkedOrder.CustomerId : null,
                OrderCustomerName = r.LinkedOrder != null && r.LinkedOrder.Customer != null
                    ? r.LinkedOrder.Customer.FullName
                    : null,
                OrderCustomerPhone = r.LinkedOrder != null && r.LinkedOrder.Customer != null
                    ? r.LinkedOrder.Customer.PhoneNumber
                    : null,
                OrderSaleDate = r.LinkedOrder != null ? r.LinkedOrder.SaleDate : null,
                OrderSellAmount = r.LinkedOrder != null ? r.LinkedOrder.SellAmount : null
            })
            .ToListAsync(ct);

        var owned = await KeepOwnedRowsAsync(rows, list.Id, list.BranchId, ct);
        var today = clock.Today;
        var orders = owned
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Select(ToDto)
            .ToList();

        await AttachShopRatingsAsync(owned, orders, ct);
        await MarkNewOrOldAsync(owned, orders, list.BranchId, ct);
        await AttachPaymentRatingsAsync(orders, today, ct);

        var result = new DelegateSalesOrdersDto { ListId = list.Id };
        DelegateSalesOrdersRules.Summarize(result, orders, today);
        return result;
    }

    private async Task<List<RequestRow>> KeepOwnedRowsAsync(
        List<RequestRow> rows,
        int listId,
        int branchId,
        CancellationToken ct)
    {
        var creatorIds = rows
            .Where(r => r.LinkedOrderListId is null && r.CreatedByUserId is not null)
            .Select(r => r.CreatedByUserId!.Value)
            .Distinct()
            .ToList();

        var creatorLists = creatorIds.Count == 0
            ? []
            : await db.OrderLists.AsNoTracking()
                .Where(l => l.BranchId == branchId && creatorIds.Contains(l.MandobId))
                .Select(l => new { l.Id, l.MandobId })
                .ToListAsync(ct);

        var multiListIds = creatorLists
            .GroupBy(l => l.MandobId)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Select(l => l.Id))
            .ToList();

        var saleDatesByList = multiListIds.Count == 0
            ? new Dictionary<int, List<DateOnly>>()
            : (await db.Orders.AsNoTracking()
                    .Where(o => o.OrderListId != null
                                && multiListIds.Contains(o.OrderListId.Value)
                                && o.ApprovalStatus == OrderApprovalStatus.Approved
                                && o.SaleDate != null)
                    .Select(o => new { ListId = o.OrderListId!.Value, SaleDate = o.SaleDate!.Value })
                    .Distinct()
                    .ToListAsync(ct))
                .GroupBy(x => x.ListId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.SaleDate).OrderBy(d => d).ToList());

        var listsByCreator = creatorLists
            .GroupBy(l => l.MandobId)
            .ToDictionary(g => g.Key, g => g.Select(l => l.Id).ToList());

        var owned = new List<RequestRow>(rows.Count);
        foreach (var row in rows)
        {
            var requestDay = IraqDate(row.CreatedAt);
            var candidates = row.CreatedByUserId is int creator && listsByCreator.TryGetValue(creator, out var ids)
                ? ids
                : [];

            var ordered = DelegateSalesOrdersRules.OrderCreatorLists(candidates.Select(id =>
            {
                DateOnly? last = null;
                if (saleDatesByList.TryGetValue(id, out var dates))
                {
                    var eligible = requestDay is null ? dates : dates.Where(d => d <= requestDay.Value).ToList();
                    last = eligible.Count == 0 ? null : eligible[^1];
                }

                return (id, last);
            }));

            if (DelegateSalesOrdersRules.ResolveListId(row.LinkedOrderListId, ordered) == listId)
                owned.Add(row);
        }

        return owned;
    }

    private static DelegateSalesOrderDto ToDto(RequestRow r)
    {
        var outcome = DelegateSalesOrdersRules.Outcome(r.Status, r.IsDeleted);
        var sold = outcome == DelegateSalesOrdersRules.OutcomeSold;
        return new DelegateSalesOrderDto
        {
            RequestId = r.Id,
            CustomerName = (sold ? r.OrderCustomerName : null) ?? r.CustomerName,
            CustomerPhone = (sold ? r.OrderCustomerPhone : null) ?? r.CustomerPhone,
            CreatedAtIraq = IraqDate(r.CreatedAt),
            Outcome = outcome,
            StatusLabel = DelegateSalesOrdersRules.StatusLabel(outcome, r.Status),
            RejectionReason = outcome == DelegateSalesOrdersRules.OutcomeRejected
                ? FirstNonEmpty(r.RejectReason, r.DeleteReason)
                : null,
            TargetEmployeeName = r.TargetEmployeeName,
            CustomerId = sold ? r.OrderCustomerId : null,
            SoldAtIraq = sold ? r.OrderSaleDate ?? IraqDate(r.StatusChangedAtUtc) : null,
            FinalSalePrice = sold ? r.OrderSellAmount : null,
            IsDeclaredOld = r.ExistingCustomerId is > 0
        };
    }

    private async Task AttachShopRatingsAsync(
        IReadOnlyList<RequestRow> rows,
        IReadOnlyList<DelegateSalesOrderDto> orders,
        CancellationToken ct)
    {
        var orderIds = rows.Where(r => r.LinkedOrderId is not null).Select(r => r.LinkedOrderId!.Value).Distinct().ToList();
        if (orderIds.Count == 0)
            return;

        var ratings = await db.CustomerStaffRatings.AsNoTracking()
            .Where(x => x.Role == CustomerStaffRating.RoleSalesEmployee
                        && x.OrderId != null
                        && orderIds.Contains(x.OrderId.Value))
            .Select(x => new { OrderId = x.OrderId!.Value, x.Score, x.RatedAtUtc, x.Id })
            .ToListAsync(ct);

        var latest = ratings
            .GroupBy(x => x.OrderId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.RatedAtUtc).ThenByDescending(x => x.Id).First().Score);

        var orderIdByRequest = rows.ToDictionary(r => r.Id, r => r.LinkedOrderId);
        foreach (var order in orders)
        {
            if (orderIdByRequest.GetValueOrDefault(order.RequestId) is int orderId
                && latest.TryGetValue(orderId, out var score))
            {
                order.ShopRating = score;
                order.IsWrongSale = DelegateSalesOrdersRules.IsWrongSale(score);
            }
        }
    }

    private async Task MarkNewOrOldAsync(
        IReadOnlyList<RequestRow> rows,
        IReadOnlyList<DelegateSalesOrderDto> orders,
        int branchId,
        CancellationToken ct)
    {
        var sold = orders.Where(o => o.Outcome == DelegateSalesOrdersRules.OutcomeSold).ToList();
        if (sold.Count == 0)
            return;

        var customers = await db.Customers.AsNoTracking()
            .Where(c => c.BranchId == branchId)
            .Select(c => new DelegateSalesOrdersRules.ExistingCustomer(c.Id, c.FullName, c.PhoneNumber))
            .ToListAsync(ct);
        var index = new DelegateSalesOrdersRules.CustomerIndex(customers);
        var existingById = rows.ToDictionary(r => r.Id, r => r.ExistingCustomerId);
        var namesById = customers.ToDictionary(c => c.CustomerId, c => c.Name);

        foreach (var order in sold)
        {
            if (order.IsDeclaredOld)
            {
                var existingId = existingById.GetValueOrDefault(order.RequestId);
                order.IsNewCustomer = false;
                order.OldCustomerReason = DelegateSalesOrdersRules.DeclaredOldReason;
                order.MatchedCustomerId = existingId;
                order.MatchedCustomerName = existingId is int id ? namesById.GetValueOrDefault(id) : null;
                continue;
            }

            var hit = index.FindEarlier(order.CustomerName, order.CustomerPhone, order.CustomerId);
            if (hit is { } match)
            {
                order.IsNewCustomer = false;
                order.OldCustomerReason = match.Reason;
                order.MatchedCustomerId = match.Customer.CustomerId;
                order.MatchedCustomerName = match.Customer.Name;
            }
            else
            {
                order.IsNewCustomer = true;
            }
        }
    }

    private async Task AttachPaymentRatingsAsync(
        IReadOnlyList<DelegateSalesOrderDto> orders,
        DateOnly today,
        CancellationToken ct)
    {
        var ids = orders
            .Where(o => o.Outcome == DelegateSalesOrdersRules.OutcomeSold && o.CustomerId is > 0)
            .Select(o => o.CustomerId!.Value)
            .Distinct()
            .ToList();
        if (ids.Count == 0)
            return;

        var facts = await db.Orders.AsNoTracking()
            .Where(o => ids.Contains(o.CustomerId) && o.ApprovalStatus == OrderApprovalStatus.Approved)
            .GroupBy(o => o.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                Total = g.Sum(o => o.SellAmount),
                Receipts = g.Sum(o => o.InstallmentPayments.Sum(p => p.Amount)),
                SaleDate = g.Min(o => o.SaleDate),
                LastPayment = g.Max(o => o.InstallmentPayments.Max(p => (DateOnly?)p.Date))
            })
            .ToListAsync(ct);

        var labels = new Dictionary<int, string>();
        foreach (var fact in facts)
        {
            var score = CustomerRatingCalculator.ScoreFor(
                fact.Total,
                fact.Receipts,
                fact.Total - fact.Receipts,
                fact.SaleDate,
                fact.LastPayment,
                today,
                false);
            if (score is not null)
                labels[fact.CustomerId] = CustomerRatingCalculator.LevelName(score.Value);
        }

        foreach (var order in orders)
        {
            if (order.CustomerId is int id && labels.TryGetValue(id, out var label))
                order.PaymentRatingLabel = label;
        }
    }

    private static DateOnly? IraqDate(DateTimeOffset? value)
        => value is DateTimeOffset v ? DateOnly.FromDateTime(v.ToOffset(IraqOffset).DateTime) : null;

    private static string? FirstNonEmpty(params string?[] values)
        => values.Select(v => v?.Trim()).FirstOrDefault(v => string.IsNullOrEmpty(v) == false);

    private sealed class RequestRow
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = "";
        public string? CustomerPhone { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public SalesRequestStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public string? RejectReason { get; set; }
        public string? DeleteReason { get; set; }
        public int? ExistingCustomerId { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTimeOffset? StatusChangedAtUtc { get; set; }
        public string? TargetEmployeeName { get; set; }
        public int? LinkedOrderId { get; set; }
        public int? LinkedOrderListId { get; set; }
        public int? OrderCustomerId { get; set; }
        public string? OrderCustomerName { get; set; }
        public string? OrderCustomerPhone { get; set; }
        public DateOnly? OrderSaleDate { get; set; }
        public double? OrderSellAmount { get; set; }
    }
}
