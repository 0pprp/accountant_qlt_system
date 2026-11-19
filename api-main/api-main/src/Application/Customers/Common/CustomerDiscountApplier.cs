using Application.Common.Interfaces;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.CustomerAggregate;
using Domain.Entities.OrderAggregate;
using Domain.Entities.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Customers.Common;

public record OrderDiscountChange(
    Order Order,
    double SellDiscount,
    double DailyDiscount,
    bool CompletesOrder);

/// <summary>
/// يجسّد خصم الزبون على مبالغ طلباته المخزونة، فيرى كل قارئ في النظام الرقم الصافي
/// بلا تعديل أي استعلام. الحساب مطلق لا تراكمي: المطبَّق يُطرح للخلف قبل طرح الهدف.
/// </summary>
public static class CustomerDiscountApplier
{
    /// <summary>namespace قفل الزبون — منفصل عن 791901/791902/791903/791905.</summary>
    public const int CustomerDiscountLockNamespace = 791906;

    private const double Tolerance = 0.005;

    /// <summary>تحقّق وحساب نقيّان بلا قاعدة بيانات — كلٌّ أو لا شيء على مستوى الزبون.</summary>
    public static Result<List<OrderDiscountChange>> Plan(
        IReadOnlyCollection<Order> orders,
        double targetSellDiscount,
        double targetDailyDiscount)
    {
        var changes = new List<OrderDiscountChange>();

        foreach (var order in orders)
        {
            var grossSell = order.GrossSell();
            if (grossSell <= 0)
                continue;

            var newSell = Math.Round(grossSell - targetSellDiscount, 2);
            if (newSell <= 0)
                return CustomerErrors.CustomerDiscountExceedsOrderSellAmount;

            var paid = order.InstallmentPayments.Sum(x => x.Amount);
            var floor = Math.Max(paid, order.PrepaymentAmount);
            if (newSell < floor - Tolerance)
                return CustomerErrors.CustomerDiscountExceedsOrderRemainingAmount;

            var grossDaily = order.GrossDaily();
            var dailyDiscount = grossDaily > 0 ? targetDailyDiscount : 0;
            if (grossDaily > 0 && Math.Round(grossDaily - dailyDiscount, 2) <= 0)
                return CustomerErrors.CustomerDiscountExceedsOrderDailyInstallment;

            if (newSell > order.SellAmount + Tolerance
                && order.ExecutionStatus == OrderExecutionStatus.Completed)
            {
                return CustomerErrors.CustomerDiscountCanNotRaiseCompletedOrderAmount;
            }

            var completes = paid >= newSell - Tolerance;
            changes.Add(new OrderDiscountChange(order, targetSellDiscount, dailyDiscount, completes));
        }

        return changes;
    }

    /// <summary>
    /// يرمّم كل طلبات الزبون على خصمه الحالي. لا ينادي SaveChangesAsync — وحدة العمل بيد المُنادي.
    /// </summary>
    public static async Task<Result> ReconcileCustomerOrdersAsync(
        IApplicationDbContext dbContext,
        Customer customer,
        IActivityLogService activityLogService,
        CancellationToken cancellationToken)
    {
        await AcquireLockAsync(dbContext, CustomerDiscountLockNamespace, customer.Id, cancellationToken);

        var orders = await dbContext.Orders
            .Include(x => x.InstallmentPayments)
            .Where(x => x.CustomerId == customer.Id)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
            return Result.Success();

        foreach (var order in orders)
            await AcquireLockAsync(dbContext, OrderLockNamespace, order.Id, cancellationToken);

        var planned = Plan(orders, customer.SellAmountDiscount, customer.DailyInstallmentDiscount);
        if (planned.IsFailed)
            return planned.Error!;

        foreach (var change in planned.Data!)
        {
            var before = change.Order.SellAmount;
            change.Order.ApplyCustomerDiscounts(change.SellDiscount, change.DailyDiscount);

            if (change.CompletesOrder)
                change.Order.ExecutionStatus = OrderExecutionStatus.Completed;

            if (Math.Abs(change.Order.SellAmount - before) < Tolerance)
                continue;

            await activityLogService.AddAsync(
                change.Order.BranchId,
                ActivityType.OrderUpdated,
                TargetEntityType.Order,
                change.Order.Id,
                details: $"خصم العميل: البيع {change.SellDiscount}، القسط {change.DailyDiscount}");
        }

        return Result.Success();
    }

    /// <summary>طلب جديد — يُطبَّق عليه خصم الزبون الحالي قبل الحفظ.</summary>
    public static Result ApplyToNewOrder(Order order, Customer customer)
    {
        var planned = Plan([order], customer.SellAmountDiscount, customer.DailyInstallmentDiscount);
        if (planned.IsFailed)
            return planned.Error!;

        foreach (var change in planned.Data!)
            change.Order.ApplyCustomerDiscounts(change.SellDiscount, change.DailyDiscount);

        return Result.Success();
    }

    private const int OrderLockNamespace = 791902;

    private static async Task AcquireLockAsync(
        IApplicationDbContext dbContext,
        int lockNamespace,
        int key,
        CancellationToken cancellationToken)
    {
        if (IsNpgsql(dbContext) == false)
            return;

        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0}, {1})",
            [lockNamespace, key],
            cancellationToken);
    }

    private static bool IsNpgsql(IApplicationDbContext dbContext)
    {
        var provider = dbContext.Database.ProviderName ?? "";
        return provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);
    }
}
