namespace Application.FollowerWork.Common;

/// <summary>
/// قواعد المتدنية — مطابقة المصدر:
/// DirectorFollowerOpsDeclineRules / FollowerDeclineRules.
/// هبوط درجتين (−10) خلال 7 أيام على انتقالات محددة فقط:
/// ممتاز→ضعيف، جيد→متلكئ، ضعيف→سيء.
/// ليست فئة «ضعيف» العامة وليست كل من عليه باقي.
/// </summary>
public static class FollowerDeclineRules
{
    public const int LookbackDays = 7;
    public const int MinScoreDrop = 10;

    public const string Excellent = "ممتاز";
    public const string Good = "جيد";
    public const string Weak = "ضعيف";
    public const string Lagging = "متلكئ";
    public const string Bad = "سيء";

    public static string NormalizeBand(string? label)
    {
        var s = (label ?? "").Trim();
        if (s.Length == 0)
            return "";

        var cut = s.IndexOf(" - ", StringComparison.Ordinal);
        if (cut > 0)
            s = s[..cut].Trim();

        var sp = s.IndexOf(' ');
        if (sp > 0
            && s is not (Excellent or Good or Weak or Lagging or Bad or "قانوني"))
        {
            s = s[..sp].Trim();
        }

        // مرادفات مصدرية
        if (s is "مرفوض" or "rejected" or "lagging")
            return Lagging;
        if (s is "bad" or "سيئ")
            return Bad;
        if (s is "weak")
            return Weak;
        if (s is "excellent")
            return Excellent;
        if (s is "good")
            return Good;

        return s;
    }

    public static bool IsTargetTransition(string? fromLabel, string? toLabel)
    {
        var from = NormalizeBand(fromLabel);
        var to = NormalizeBand(toLabel);
        return
            (from == Excellent && to == Weak)
            || (from == Good && to == Lagging)
            || (from == Weak && to == Bad);
    }

    public static bool IsDeclining(
        string? fromLabel,
        int fromScore,
        string? toLabel,
        int toScore,
        int daysBetween)
    {
        if (daysBetween < 0 || daysBetween > LookbackDays)
            return false;
        if (IsTargetTransition(fromLabel, toLabel) == false)
            return false;
        return fromScore - toScore >= MinScoreDrop;
    }

    public static string LabelForScore(int score) => score switch
    {
        10 => Excellent,
        5 => Good,
        0 => Weak,
        -5 => Lagging,
        _ => Bad
    };
}
