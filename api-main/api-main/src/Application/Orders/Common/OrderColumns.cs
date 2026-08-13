using System.Linq.Expressions;
using Application.Common.Extensions;
using Domain.Entities.OrderAggregate;

namespace Application.Orders.Common;

public record OrderColumn(
    string Key,
    string DisplayName,
    string DataType,
    Expression<Func<Order, object?>> Selector);

public static class OrderColumns
{
    public static readonly List<OrderColumn> All =
    [
        new("Id", "المعرف", "number", x => x.Id),
        new("ProductsSummary", "ملخص المنتجات", "string",
            x => string.Join(", ",
                x.OrderItems.Select(oi => $"{(oi.ProductId != null ? oi.Product!.Name : oi.ProductName)} ({oi.Quantity})"))),
        new("OrderListName", "اسم القائمة", "string", x => x.OrderList!.Name),
        new("CustomerFullName", "اسم العميل", "string", x => x.Customer!.FullName),
        new("CustomerPhoneNumber", "رقم هاتف العميل", "string", x => x.Customer!.PhoneNumber),
        new("LastInstallmentDate", "تاريخ آخر قسط", "date", x => x.InstallmentPayments.Max(ip => ip.Date)),
        new("InstallmentsCount", "عدد الأقساط", "number", x => x.InstallmentPayments.Count),
        new("TotalInstallmentsAmount", "إجمالي الأقساط المدفوعة", "number", x => x.InstallmentPayments.Sum(ip => ip.Amount)),
        new("RemainingAmount", "المبلغ المتبقي", "number", x => x.SellAmount - x.InstallmentPayments.Sum(ip => ip.Amount)),
        new("SellAmount", "مبلغ البيع", "number", x => x.SellAmount),
        new("SellerName", "اسم البائع", "string", x => x.Seller!.FullName),
        new("SaleDate", "تاريخ البيع", "date", x => x.SaleDate),
        new("CreatedAt", "تاريخ الإنشاء", "datetime", x => x.CreatedAt!.Value),
        new("BuyAmount", "مبلغ الشراء", "number", x => x.BuyAmount),
        new("PrepaymentAmount", "مبلغ الدفعة المقدمة", "number", x => x.PrepaymentAmount),
        new("DailyInstallmentAmount", "مبلغ القسط اليومي", "number", x => x.DailyInstallmentAmount),
        new("ExecutionStatus", "حالة التنفيذ", "string", x => x.ExecutionStatus.ToArabicString()),
        new("ApprovalStatus", "حالة الموافقة", "string", x => x.ApprovalStatus.ToArabicString()),
        new("CustomerNationalCode", "الرقم الوطني للعميل", "string", x => x.Customer!.NationalCode)
    ];

    public static IEnumerable<string> GetAllKeys() => All.Select(c => c.Key);

    public static OrderColumn? GetField(string key) =>
        All.FirstOrDefault(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
}