namespace Application.FollowerWork.Common;

/// <summary>مطابق FollowerSaleAuthRules في المصدر القديم.</summary>
public static class FollowerSaleAuthRules
{
    public const int MinRating = 1;
    public const int MaxRating = 10;
    public const int OptionalNoteMinRating = 7;
    public const int WrongSaleMaxRating = 6;
    public const double RemainingBand100k = 100_000;
    public const double RemainingBand200k = 200_000;

    public static bool IsValidRating(int rating) =>
        rating >= MinRating && rating <= MaxRating;

    public static bool IsValidNoteForRating(int rating, string? note)
    {
        if (IsValidRating(rating) == false)
            return false;
        if (rating >= OptionalNoteMinRating)
            return true;
        return string.IsNullOrWhiteSpace(note) == false;
    }

    public static bool IsLowAuthRisk(int? authRating) =>
        authRating is >= MinRating and <= WrongSaleMaxRating;

    /// <summary>تقييم مصادقة ≤6 يدخل مسار المبيعات الخاطئة (ليس رفض SalesRequest).</summary>
    public static bool IsWrongSaleRating(int rating) =>
        rating >= MinRating && rating <= WrongSaleMaxRating;

    public static string FormatWrongSaleReason(
        int rating,
        string? note,
        decimal stock,
        decimal revenue,
        string? customerName,
        string? phone)
    {
        var n = NormalizeNote(note) ?? "—";
        return $"مبيعات خاطئة\nالزبون: {customerName ?? "—"}\nالهاتف: {phone ?? "—"}\nالتقييم: {rating}\nالملاحظة: {n}\nبضاعة: {stock:0}\nوارد يومي: {revenue:0}";
    }

    public static bool IsValidShopMoney(decimal value) => value > 0;

    public static string? NormalizeNote(string? note)
    {
        var t = (note ?? string.Empty).Trim();
        return t.Length == 0 ? null : t;
    }

    public static readonly HashSet<string> RemainingBuckets = new(StringComparer.OrdinalIgnoreCase)
    {
        "excellent", "good", "weak", "lagging", "bad",
        "le100", "lt200", "rated",
    };

    public static bool TryNormalizeBucket(string? bucket, out string normalized)
    {
        var key = (bucket ?? "rated").Trim().ToLowerInvariant();
        key = key switch
        {
            "ممتاز" => "excellent",
            "جيد" => "good",
            "ضعيف" => "weak",
            "متلكئ" or "rejected" => "lagging",
            "سيء" => "bad",
            "100" or "100k" => "le100",
            "200" or "200k" => "lt200",
            "ratings" or "all" or "" => "rated",
            _ => key
        };
        if (RemainingBuckets.Contains(key) == false)
        {
            normalized = "rated";
            return false;
        }

        normalized = RemainingBuckets.First(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
        return true;
    }
}
