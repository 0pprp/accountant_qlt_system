namespace Application.ManagementSummary.Common;

public static class ManagementSummaryRules
{
    public const string TimeZoneId = "Asia/Baghdad";
    public const string Currency = "IQD";
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    public static string MetricLabel(ManagementSummaryMetric metric) => metric switch
    {
        ManagementSummaryMetric.Receipts => "الواردات",
        ManagementSummaryMetric.Expenses => "الصرفيات",
        ManagementSummaryMetric.SalePrice => "سعر البيع",
        ManagementSummaryMetric.PurchasePrice => "سعر الشراء",
        ManagementSummaryMetric.Buys => "المشتريات",
        _ => metric.ToString(),
    };

    public static int ClampPageSize(int? pageSize)
    {
        if (pageSize is null or <= 0)
            return DefaultPageSize;
        return Math.Min(pageSize.Value, MaxPageSize);
    }

    public static int ClampPage(int? page) => page is null or < 1 ? 1 : page.Value;

    public static bool IsValidPeriod(int year, int month)
        => year is >= 2000 and <= 2100 && month is >= 1 and <= 12;

    public static bool IsValidDay(int year, int month, int day)
        => IsValidPeriod(year, month) && day >= 1 && day <= IraqCalendar.DaysInMonth(year, month);
}
