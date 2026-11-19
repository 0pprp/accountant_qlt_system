namespace Domain.Entities.SafeAggregate;

/// <summary>أسماء الخزائن مشتقة من مصدرها — تُعاد التسمية عند تغيير اسم المصدر.</summary>
public static class SafeNames
{
    public const int MaxLength = 100;

    public static string ForBranch(string branchName) => Trim($"قاصة {branchName.Trim()}");

    public static string ForExpenses(string branchName) => Trim($"سجل الصرفيات — {branchName.Trim()}");

    public static string ForOrderList(string orderListName) => Trim(orderListName.Trim());

    public static string ForEmployee(string employeeName) => Trim(employeeName.Trim());

    public static string ForMisc(string safeName) => Trim(safeName.Trim());

    private static string Trim(string value) =>
        value.Length <= MaxLength ? value : value[..MaxLength];
}
