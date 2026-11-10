namespace Application.ManagementSummary.Common;

public enum ManagementSummaryMetric
{
    Receipts = 1,
    Expenses = 2,
    SalePrice = 3,
    PurchasePrice = 4,
    Buys = 5,
}

public static class ManagementSummaryMetricParser
{
    public static bool TryParse(string? value, out ManagementSummaryMetric metric)
    {
        metric = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        switch (value.Trim().ToLowerInvariant())
        {
            case "receipts":
                metric = ManagementSummaryMetric.Receipts;
                return true;
            case "expenses":
                metric = ManagementSummaryMetric.Expenses;
                return true;
            case "saleprice":
                metric = ManagementSummaryMetric.SalePrice;
                return true;
            case "purchaseprice":
                metric = ManagementSummaryMetric.PurchasePrice;
                return true;
            case "buys":
                metric = ManagementSummaryMetric.Buys;
                return true;
            default:
                return Enum.TryParse(value.Trim(), ignoreCase: true, out metric)
                       && Enum.IsDefined(typeof(ManagementSummaryMetric), metric);
        }
    }

    public static string ToApiKey(ManagementSummaryMetric metric) => metric switch
    {
        ManagementSummaryMetric.Receipts => "receipts",
        ManagementSummaryMetric.Expenses => "expenses",
        ManagementSummaryMetric.SalePrice => "salePrice",
        ManagementSummaryMetric.PurchasePrice => "purchasePrice",
        ManagementSummaryMetric.Buys => "buys",
        _ => metric.ToString(),
    };
}
