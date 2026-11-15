using Application.SalesRatings.Common;

namespace Application.ManagementSalesMap.Common;

public static class SalesMapStatusRules
{
    public const string LabelContinuous = "مستمر";
    public const string LabelStopped = "متوقف";
    public const string LabelLegal = "قانونية";
    public const string LabelUnknown = "غير محدد";

    public static (int? Code, string Label) Classify(
        bool isLegal,
        DateOnly? lastPaymentDate,
        DateOnly? saleDate,
        DateOnly asOf)
    {
        if (isLegal)
            return ((int)SalesMapPointStatusCode.Legal, LabelLegal);

        if (ActiveCustomerRules.IsActive(lastPaymentDate, asOf))
            return ((int)SalesMapPointStatusCode.Continuous, LabelContinuous);

        if (lastPaymentDate is null && saleDate is null)
            return (null, LabelUnknown);

        return ((int)SalesMapPointStatusCode.Stopped, LabelStopped);
    }

    public static bool MatchesFilter(int? statusCode, SalesMapStatusFilter filter) =>
        filter switch
        {
            SalesMapStatusFilter.All => true,
            SalesMapStatusFilter.Continuous => statusCode == (int)SalesMapPointStatusCode.Continuous,
            SalesMapStatusFilter.Stopped => statusCode == (int)SalesMapPointStatusCode.Stopped,
            SalesMapStatusFilter.Legal => statusCode == (int)SalesMapPointStatusCode.Legal,
            _ => true
        };

    public static string? RatingTierKey(string? levelName) => levelName switch
    {
        "ممتاز" => "excellent",
        "جيد" => "good",
        "ضعيف" => "weak",
        "متلكئ" => "lagging",
        "سيء" => "bad",
        _ => levelName is null or "" ? null : "unknown"
    };
}
