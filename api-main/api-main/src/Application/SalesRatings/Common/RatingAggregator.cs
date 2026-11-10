namespace Application.SalesRatings.Common;

public sealed record RatingAggregateResult(
    int CustomerCount,
    int ExcellentCount,
    int GoodCount,
    int WeakCount,
    int LaggingCount,
    int BadCount,
    int TotalPoints,
    double AverageScore,
    string FinalRatingLabel);

/// <summary>
/// مطابق ListRatingAggregator القديم — عتبات متوسط القائمة.
/// مشترك مع DelegateFollowupRules.ClassifyAverageLabel.
/// </summary>
public static class RatingAggregator
{
    public static RatingAggregateResult Aggregate(IEnumerable<int> scores)
    {
        var list = scores as IList<int> ?? scores.ToList();
        if (list.Count == 0)
        {
            return new RatingAggregateResult(0, 0, 0, 0, 0, 0, 0, 0, ClassifyAverageLabel(0));
        }

        var excellent = 0;
        var good = 0;
        var weak = 0;
        var lagging = 0;
        var bad = 0;
        var total = 0;
        foreach (var score in list)
        {
            total += score;
            switch (CustomerRatingCalculator.LevelName(score))
            {
                case "ممتاز": excellent++; break;
                case "جيد": good++; break;
                case "ضعيف": weak++; break;
                case "متلكئ": lagging++; break;
                default: bad++; break;
            }
        }

        var average = (double)total / list.Count;
        return new RatingAggregateResult(
            list.Count,
            excellent,
            good,
            weak,
            lagging,
            bad,
            total,
            average,
            ClassifyAverageLabel(average));
    }

    public static string ClassifyAverageLabel(double averageScore)
    {
        if (averageScore >= 7) return "ممتاز";
        if (averageScore >= 3) return "جيد";
        if (averageScore >= 0) return "ضعيف";
        if (averageScore > -7.5) return "متلكئ";
        return "سيء";
    }

    public static string DirectionFromSeries(double start, double end)
    {
        var abs = end - start;
        if (abs > 0.05) return "up";
        if (abs < -0.05) return "down";
        return "flat";
    }
}
